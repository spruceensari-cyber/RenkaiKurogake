
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Renkai.Kurogake;

public static class RenkaiV24RoundInstaller
{
    [MenuItem("Renkai/V2.4 Install 5v5 Round Bomb Mode")]
    public static void Install()
    {
        Safety();
        GameObject player = Player();
        Teams();
        Bomb();
        HUD(player);
        EditorUtility.DisplayDialog("Renkai V2.4", "5v5 Round + Bomb Mode kuruldu.", "OK");
    }

    static void Safety()
    {
        Cube("V2_4_Floor", new Vector3(0,-.25f,0), new Vector3(160,.35f,160), new Color(.02f,.025f,.04f));
        Cube("V2_4_North", new Vector3(0,4,78), new Vector3(160,8,4), new Color(.025f,.03f,.055f));
        Cube("V2_4_South", new Vector3(0,4,-78), new Vector3(160,8,4), new Color(.025f,.03f,.055f));
        Cube("V2_4_West", new Vector3(-78,4,0), new Vector3(4,8,160), new Color(.025f,.03f,.055f));
        Cube("V2_4_East", new Vector3(78,4,0), new Vector3(4,8,160), new Color(.025f,.03f,.055f));
    }

    static GameObject Player()
    {
        GameObject p = GameObject.Find("Demo_Player_FPS_Controller");
        if (!p) p = GameObject.Find("Demo_Player_Renkai_Controller");
        if (!p){ Debug.LogError("Önce çalışan Renkai map sahnesini generate et."); return null; }
        p.tag = "Player";
        var rp = p.GetComponent<RenkaiRoundPlayer>(); if(!rp) rp = p.AddComponent<RenkaiRoundPlayer>();
        rp.agentName = "You / Empress of Violet"; rp.team = RenkaiTeam.Attackers; rp.isHumanPlayer = true; rp.RememberSpawn();
        if(!p.GetComponent<RenkaiHealth>()) p.AddComponent<RenkaiHealth>();
        if(!p.GetComponent<BombPlanter>()) p.AddComponent<BombPlanter>();
        var w = p.GetComponent<RenkaiWeaponController>(); if(!w) w = p.AddComponent<RenkaiWeaponController>();
        w.playerCamera = p.GetComponentInChildren<Camera>();
        ViewModels(p, w);
        return p;
    }

    static void ViewModels(GameObject p, RenkaiWeaponController w)
    {
        Camera cam = p.GetComponentInChildren<Camera>(); if(!cam) return;
        w.rifleView = View("Rifle_View", cam.transform, new Vector3(.38f,-.28f,.65f), new Vector3(.22f,.16f,.9f), Color.black);
        w.pistolView = View("Pistol_View", cam.transform, new Vector3(.32f,-.24f,.55f), new Vector3(.16f,.13f,.45f), Color.black);
        w.swordView = View("Sword_View", cam.transform, new Vector3(-.34f,-.25f,.70f), new Vector3(.08f,.08f,1.05f), Color.cyan);
    }

    static GameObject View(string n, Transform parent, Vector3 pos, Vector3 scale, Color c)
    {
        Transform old = parent.Find(n); if(old) Object.DestroyImmediate(old.gameObject);
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = n; go.transform.SetParent(parent); go.transform.localPosition = pos; go.transform.localRotation = Quaternion.identity; go.transform.localScale = scale; Paint(go,c,.6f); return go;
    }

    static void Teams()
    {
        var old = GameObject.Find("V2_4_5v5_Teams"); if(old) Object.DestroyImmediate(old);
        var root = new GameObject("V2_4_5v5_Teams");
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Renkai/Characters/EmpressOfViolet/EmpressOfViolet_Player.glb");
        for(int i=0;i<4;i++) Agent("Ally_"+(i+1), RenkaiTeam.Attackers, new Vector3(-6+i*4,.1f,-61), root.transform, model, new Color(.2f,.5f,1f));
        for(int i=0;i<5;i++) Agent("Enemy_"+(i+1), RenkaiTeam.Defenders, new Vector3(-10+i*5,.1f,48+(i%2)*4), root.transform, model, new Color(1f,.18f,.45f));
    }

    static void Agent(string n, RenkaiTeam team, Vector3 pos, Transform parent, GameObject model, Color color)
    {
        GameObject a;
        if(model){ a = (GameObject)PrefabUtility.InstantiatePrefab(model); a.transform.position = pos; }
        else { a = GameObject.CreatePrimitive(PrimitiveType.Capsule); a.transform.position = pos; Paint(a,color,.5f); }
        a.name = n; a.transform.SetParent(parent); a.transform.rotation = Quaternion.Euler(0, team==RenkaiTeam.Attackers?0:180, 0);
        if(!a.GetComponent<RenkaiHealth>()) a.AddComponent<RenkaiHealth>();
        var rp = a.GetComponent<RenkaiRoundPlayer>(); if(!rp) rp = a.AddComponent<RenkaiRoundPlayer>();
        rp.agentName = n; rp.team = team; rp.isHumanPlayer = false; rp.RememberSpawn();
        var hb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hb.name = "Hitbox"; hb.transform.SetParent(a.transform); hb.transform.localPosition = new Vector3(0,.9f,0); hb.transform.localScale = new Vector3(.8f,1f,.8f);
        hb.GetComponent<Renderer>().enabled = false; hb.AddComponent<RenkaiHealth>();
    }

    static void Bomb()
    {
        var old = GameObject.Find("V2_4_Bomb_Core"); if(old) Object.DestroyImmediate(old);
        var bombObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bombObj.name = "V2_4_Bomb_Core"; bombObj.transform.position = new Vector3(-42,.25f,15); bombObj.transform.localScale = new Vector3(.5f,.18f,.5f); Paint(bombObj, new Color(.65f,.1f,1f),2f);
        var bomb = bombObj.AddComponent<RenkaiBombCore>(); bombObj.SetActive(false);
        var mgrOld = GameObject.Find("V2_4_Round_Manager"); if(mgrOld) Object.DestroyImmediate(mgrOld);
        var mgrObj = new GameObject("V2_4_Round_Manager"); var mgr = mgrObj.AddComponent<RenkaiRoundManager>(); mgr.bomb = bomb;
        Site("A_Site_Trigger", new Vector3(-42,1.5f,15), new Vector3(24,3,22), "A");
        Site("B_Site_Trigger", new Vector3(42,1.5f,15), new Vector3(22,3,20), "B");
    }

    static void Site(string n, Vector3 pos, Vector3 size, string site)
    {
        if(GameObject.Find(n)) return;
        var go = new GameObject(n); go.transform.position = pos; var col = go.AddComponent<BoxCollider>(); col.isTrigger=true; col.size=size;
        var z = go.AddComponent<BombSiteZone>(); z.siteName = site;
    }

    static void HUD(GameObject player)
    {
        var old = GameObject.Find("V2_4_Round_HUD"); if(old) Object.DestroyImmediate(old);
        var c = new GameObject("V2_4_Round_HUD"); var canvas = c.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; c.AddComponent<CanvasScaler>(); c.AddComponent<GraphicRaycaster>();
        Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        Text status = Text(c.transform,"Status","BUY PHASE",24,new Vector2(.5f,1),new Vector2(0,-42),new Vector2(650,46),TextAnchor.MiddleCenter,f);
        Text score = Text(c.transform,"Score","ATK 0 - 0 DEF",22,new Vector2(.5f,1),new Vector2(0,-82),new Vector2(650,40),TextAnchor.MiddleCenter,f);
        Text timer = Text(c.transform,"Timer","ROUND 100",22,new Vector2(.5f,1),new Vector2(0,-122),new Vector2(650,40),TextAnchor.MiddleCenter,f);
        Text ammo = Text(c.transform,"Ammo","30 / 120",24,new Vector2(1,0),new Vector2(-120,42),new Vector2(220,60),TextAnchor.MiddleRight,f);
        Text weapon = Text(c.transform,"Weapon","Rifle",18,new Vector2(1,0),new Vector2(-120,82),new Vector2(220,40),TextAnchor.MiddleRight,f);
        Text hit = Text(c.transform,"Hit","HIT",24,new Vector2(.5f,.5f),new Vector2(0,42),new Vector2(160,50),TextAnchor.MiddleCenter,f); hit.enabled=false;
        Text(c.transform,"Controls","1 Rifle | 2 Pistol | 3 Sword | Mouse1 Fire/Slash | R Reload | F Plant/Defuse",16,new Vector2(.5f,0),new Vector2(0,34),new Vector2(980,40),TextAnchor.MiddleCenter,f);
        foreach(var t in c.GetComponentsInChildren<Text>()) t.color = new Color(.88f,.76f,1f,1f);
        if(player){ var wc = player.GetComponent<RenkaiWeaponController>(); if(wc){ wc.ammoText=ammo; wc.hitText=hit; wc.weaponText=weapon; } }
        var mgr = Object.FindObjectOfType<RenkaiRoundManager>(); if(mgr){ mgr.statusText=status; mgr.scoreText=score; mgr.timerText=timer; }
    }

    static Text Text(Transform p,string n,string v,int s,Vector2 a,Vector2 pos,Vector2 dim,TextAnchor align,Font f)
    {
        var go = new GameObject(n); go.transform.SetParent(p); var t = go.AddComponent<Text>(); t.text=v; t.font=f; t.fontSize=s; t.alignment=align;
        var rt = go.GetComponent<RectTransform>(); rt.anchorMin=a; rt.anchorMax=a; rt.anchoredPosition=pos; rt.sizeDelta=dim; return t;
    }

    static GameObject Cube(string n, Vector3 p, Vector3 s, Color c){ var old=GameObject.Find(n); if(old) Object.DestroyImmediate(old); var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=n; go.transform.position=p; go.transform.localScale=s; Paint(go,c,.4f); return go; }
    static void Paint(GameObject go, Color c, float e){ var sh=Shader.Find("Universal Render Pipeline/Lit"); if(!sh) sh=Shader.Find("Standard"); var m=new Material(sh); if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",c); if(m.HasProperty("_Color")) m.SetColor("_Color",c); m.EnableKeyword("_EMISSION"); if(m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor",c*e); go.GetComponent<Renderer>().sharedMaterial=m; }
}
