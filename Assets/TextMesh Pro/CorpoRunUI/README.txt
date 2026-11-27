CorpoRun - interfejs gry (Unity 2021.3 lub nowszy, także Unity 6; uGUI + TextMeshPro)

INSTALACJA
1. Jeśli masz poprzednią paczkę: usuń folder CorpoRunShop i obiekt ShopCanvas ze sceny.
2. Przeciągnij folder CorpoRunUI do okna Project (do Assets).
3. Otwórz scenę gry i wybierz w górnym pasku: CorpoRun -> Zbuduj interfejs gry.
   Jeśli wyskoczy "TMP Importer": Import TMP Essentials, usuń GameCanvas i ShopCanvas ze sceny i zbuduj jeszcze raz.
4. Powstaną GameCanvas (menu, HUD, pauza, koniec gry, opcje) i ShopCanvas (sklep). Gra startuje w menu.

PODPIĘCIE DO GRY (obiekt GameUI w GameCanvas)
- Zdarzenia w Inspectorze: On Play (start biegu), On Restart (nowy bieg), On Continue (wskrzeszenie po reklamie),
  On Quit To Menu (przerwanie biegu), On Pause / On Resume.
- Z kodu (using CorpoRun.UI;):
    GameUI.Instance.Hud.SetScore(wynik);
    GameUI.Instance.Hud.AddCoins(1);
    GameUI.Instance.Hud.AddFiles(1);
    GameUI.Instance.Hud.ShowPowerUp("magnet", sekundy);
    GameUI.Instance.Hud.SetSecurity(0.4f);   // 0 = ochrona daleko, 1 = zaraz złapie
    GameUI.Instance.GameOver();              // monety z biegu same trafiają do portfela, rekord sam się zapisuje
- Ustawienia z Opcji: GameSettings.MusicVolume / SfxVolume (albo komponent VolumeBinding na AudioSource),
  GameSettings.Vibrate(), GameSettings.SwipeThresholdMultiplier, GameSettings.Graphics.

PODGLĄD I ZMIANY
- Ekrany włączasz i wyłączasz w hierarchii: GameCanvas -> GameUI -> MainMenu, HUD, Pause, GameOver, Options.
  Na starcie gry GameUI i tak pokaże samo menu.
- Logo w menu: 6 wersji w Sprites/Logo, podmień sprite w obiekcie MainMenu -> Logo.
- Misja dnia w menu to wzór: MainMenuUI.SetDailyMission(...) albo wyłącz obiekt DailyMission.
- Ceny i wartości power-upów: ShopCatalog.asset. Testy sklepu: prawy klik na komponencie ShopUI.
- Fonty: Archivo Black i Inter, licencja SIL Open Font License (pliki OFL-*.txt w Fonts).
