RENKAI: KUROGAKE DISTRICT V2.2.1 - COMPILE FIX
==============================================

BU SÜRÜM NEYİ DÜZELTİYOR?
- V2.2'deki şu compile hatası düzeltildi:
  CS0103: The name 'CreateRespawnPoint' does not exist in the current context

KULLANIM:
1) ZIP'i çıkar.
2) Unity Hub ile bu klasörü ayrı proje olarak aç.
3) Unity üst menüden:
   Renkai > Create Kurogake District V2.2.1 Compile Fix Scene
4) Sahne oluşunca Play'e bas.

KONTROLLER:
WASD = hareket
Mouse = bakış
Sol tık = ateş
V = alternatif ateş testi
C veya Left Ctrl = eğilme
Shift = koşma
Space = zıplama
F = Site içinde plant testi
R = manuel respawn
ESC = mouse kilidini aç

TEST LİSTESİ:
- Console'da kırmızı compile error kalmamalı.
- Renkai menüsü görünmeli.
- Scene generate olmalı.
- Play modunda hareket, eğilme, ateş ve respawn test edilmeli.


RENKAI: KUROGAKE DISTRICT V2.2 - MOVEMENT & COMBAT FIX
======================================================

KULLANIM:
1) ZIP'i çıkar.
2) Unity Hub ile bu klasörü ayrı proje olarak aç.
3) Unity üst menü:
   Renkai > Create Kurogake District V2.2 Movement Combat Fix Scene
4) Play'e bas.

KONTROLLER:
WASD = hareket
Mouse = bakış
Sol tık = ateş
V = alternatif ateş testi
C veya Left Ctrl = eğilme
Shift = koşma
Space = zıplama
F = Site içinde plant testi
R = manuel respawn
ESC = mouse kilidini aç

V2.2 DÜZELTMELER:
- Ateş artık görünür mor tracer çizgisi oluşturur.
- Muzzle flash eklendi.
- Dummy vurunca HIT yazısı çıkar.
- C / Left Ctrl ile crouch eklendi.
- Harita dışına düşünce Y=-12 altında respawn olur.
- R tuşu ile manuel respawn eklendi.
- Güvenlik zemini eklendi.
- Player weapon script bağlantısı güçlendirildi.


RENKAI: KUROGAKE DISTRICT V2.2 - MOVEMENT & COMBAT FIX
======================================================

Bu paket, Renkai FPS oyunu için Unity'de otomatik kurulabilen oynanabilir 3D map başlangıç paketidir.

ÖNEMLİ:
- Bu paket Unity içinde sahneyi otomatik üretir.
- Burada Unity çalıştırılıp canlı test edilemedi.
- Ama scriptler, klasör yapısı ve generator Unity Editor içinde çalışacak şekilde hazırlanmıştır.
- Hedef: final kaliteye giden V2 playable map iskeleti. AAA/final shipping polish için Blender asset, PBR texture, VFX ve playtest gerekir.

KULLANIM:
1) ZIP'i çıkar.
2) Unity Hub > Add/Open ile RenkaiKurogakeDistrictV2_Unity klasörünü aç.
3) Unity import işlemi bitince üst menüden:
   Renkai > Create Kurogake District V2.1.1 Pink Fix Scene
4) Sahne şuraya kaydedilir:
   Assets/Renkai/Maps/KurogakeDistrict/Scenes/Renkai_KurogakeDistrict_V2_1_1_PinkFix.unity
5) Play'e bas.
6) Kontroller:
   WASD = hareket
   Mouse = bakış
   Shift = koşma
   Space = zıplama
   F = A/B Site içinde plant testi
   ESC = mouse kilidini aç

PAKET İÇERİĞİ:
- Procedural Unity scene generator
- FPS demo controller
- Bomb site trigger sistemi
- Kurogate teleport sistemi
- 3D map: A Site, B Site, Mid, A Main, B Main, connectors
- 3D duvarlar, yüksek platformlar, kapılar, tapınak çatılı binalar
- Neon materyaller, emissive parçalar, mor/mavi ışıklar, sis/fog
- Kurogate portal objeleri
- Top-down map görseli
- Minimap overhead kamera
- Release kalitesine ilerlemek için production notes

BU V2 NEYİ TEMSİL EDER?
- Greybox değil; duvarlı, çatılı, neonlu, oynanabilir 3D sahne oluşturur.
- Yine de gerçek final/piyasaya sürülebilir kalite için şu işler gerekir:
  Blender/Maya modular asset üretimi, UV unwrap, PBR texture, light bake, occlusion culling,
  LOD, prop polish, gerçek VFX, ses, multiplayer test, hitreg ve performans optimizasyonu.

SONRAKİ AŞAMA:
- V3 Art Pass: gerçek model paketleri ile primitive objeleri değiştirmek.
- V4 Gameplay Pass: silah, round, plant/defuse, takım sistemi.
- V5 Multiplayer Pass: Photon/FishNet/Mirror ile online test.


V2.1 COMBAT UPDATE:
- Sol tık ile çalışan basit raycast rifle sistemi eklendi.
- Target dummy sistemi eklendi.
- RenkaiHealth sistemi eklendi.
- Crosshair + round timer + control info HUD eklendi.
- Kurogate, plant trigger ve FPS controller aynı kaldı.
- Bu sürüm artık sadece gezilen map değil; ateş etme ve hedef vurma testi de içerir.

SÜRÜM STRATEJİSİ:
- V2.1: Combat sandbox.
- V2.2: Round system + plant/defuse UI.
- V2.3: Better weapon recoil + hit marker + sound placeholders.
- V2.4: Character skill framework Q/E/R.
- V3.0: Art pass; gerçek modular 3D asset değişimi.


V2.1.1 PINK MATERIAL FIX:
- Unity’de her yer pembe görünme sorunu için shader seçimi düzeltildi.
- Generator artık render pipeline aktifse URP shader, değilse Built-in Standard shader kullanır.
- Ek menü eklendi: Renkai > Fix Pink Materials In Current Scene
- Eski oluşturulmuş sahnede pembe sorununu düzeltmek için bu menüye basabilirsin.
- En temiz yol: Yeni sahneyi V2.1.1 menüsünden tekrar generate etmek.
