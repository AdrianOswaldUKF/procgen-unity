# Bakalárska práca
# Procedurálne generovanie obsahu v prostredí Unity
## Adrián Oswald
## Školiteľ: Mgr. Matúš Valko

Práca sa zameriava na návrh, implementáciu a porovnanie vybraných algoritmov procedurálneho generovania v prostredí Unity. Cieľom je analyzovať ich správanie z hľadiska výkonu, stability a vizuálneho výstupu v jednotných podmienkach. V rámci riešenia sú implementované štyri prístupy, konkrétne Perlinov šum, L-systém, celulárne automaty a kolaps vlnovej funkcie(WFC). Každý algoritmus je spracovaný vo viacerých variantoch a testovaný pomocou jednotného používateľského rozhrania a systému merania výkonu. Výsledky ukazujú, že jednotlivé metódy sa líšia vhodnosťou použitia, pričom Perlinov šum je efektívny pre plynulé štruktúry, L-systém pre organické tvary, celulárne automaty poskytujú variabilné výsledky a kolaps vlnovej funkcie(WFC) umožňuje generovanie koherentných modulárnych priestorov za cenu vyššej výpočtovej náročnosti. Práca poskytuje prehľad vlastností týchto algoritmov a ich praktického využitia.

Kľúčové slová: Procedurálne generovanie, algoritmy, Unity, Perlinov šum, L-systém, Celulárne automaty, Kolaps vlnovej funkcie

---

## Obsah

- [Implementácia algoritmov](#implementácia-algoritmov)
- [Celulárne automaty](#celulárne-automaty)
- [L-systémy](#l-systémy)
- [Perlinov šum](#perlinov-šum)
- [WFC](#wfc)
- [Základné systémy](#základné-systémy)

---

# IMPLEMENTÁCIA ALGORITMOV

---

## Celulárne automaty

### `Scripts/CellularAutomata/CaGenerator.cs`

Abstraktná rodičovská trieda pre všetky varianty celulárnych automatov.

**Účel:**

- riadi generovanie dvojrozmernej mriežky (angl. grid),
- načítava hodnoty z používateľského rozhrania,
- spúšťa výpočet buniek,
- zabezpečuje odstránenie predchádzajúcich prefabov,
- poskytuje spoločné pomocné metódy pre všetky varianty CA.

**Verejné premenné:**

- `width`, `height` - rozmery mriežky,
- `cellSize` - veľkosť jednej bunky,
- `seed` - počiatočná hodnota pre opakovateľné generovanie,
- `widthInput`, `heightInput`, `cellSizeInput`, `seedInput` - vstupy používateľského rozhrania,
- `fillProbabilitySlider` - posuvník pre počiatočné zaplnenie,
- `iterationsInput`, `birthThresholdInput` - vstupy pre algoritmus,
- `parent` - rodičovský objekt pre generované prefaby.

**Hlavné metódy:**

- `Generate()` - hlavný vstup pre generovanie,
- `ClearParentContext()` - vyčistenie vygenerovaných objektov z kontextového menu,
- `ReadUIInputs()` - načítanie parametrov z používateľského rozhrania,
- `InitializeGrid()` - vytvorenie počiatočnej mriežky,
- `GenerateGrid()` - inicializácia mriežky náhodnými hodnotami podľa pravdepodobnosti zaplnenia,
- `ApplyIterations()` - spustenie iterácií,
- `Step()` - jeden krok simulácie CA,
- `CountNeighbors()` - spočítanie susedov,
- `CountDeadEnds()` - počet slepých uličiek,
- `CountRegions()` - počet regiónov,
- `CalculateFillPct()` - výpočet percentuálnej zaplnenosti.

---

### `Scripts/CellularAutomata/CaCave.cs`

Variant celulárnych automatov určený na generovanie jaskýň.

- Dedí z rodičovskej triedy `CaGenerator`.

**Účel:**

- generuje jaskynný priestor, ako mriežku prefabov,
- používa základné pravidlá CA bez špeciálneho zániku.

**Verejné premenné:**

- `prefab` - prefab použitý pre živé bunky,
- `caveHeight` - výška jedného jaskynného bloku,
- `fillProbability` - počiatočná pravdepodobnosť zaplnenia,
- `iterations` - počet iterácií,
- `birthThreshold` - prah pre prežitie a vznik bunky.

**Hlavné metódy:**

- `ReadUIInputs()` - načítanie parametrov z používateľského rozhrania,
- `PlacePrefabs()` - umiestnenie prefabov do scény,
- `SetupUI()` - nastavenie predvolených hodnôt v používateľskom rozhraní.

---

### `Scripts/CellularAutomata/CaBiome.cs`

Variant CA na generovanie vrstiev biómov na teréne.

- Dedí z rodičovskej triedy `CaGenerator`.

**Účel:**

- vytvára kombináciu viacerých vrstiev na objekte `Terrain`,
- rozlišuje vodu, pôdu a zaplnené bunky cez alfa mapy.

**Verejné premenné:**

- `terrain` - objekt `Terrain` použitý, ako výstup,
- `fillProbability`, `iterations`, `birthThreshold` - parametre CA.

**Hlavné metódy:**

- `ReadUIInputs()` - načítanie parametrov,
- `PlacePrefabs()` - generuje a aplikuje vrstvy alfa mapy (textúry) na terén,
- `CountWaterNeighbors()` - počítanie susedov pre vodnú vrstvu,
- `ResetTerrain()` - vymazanie výšok terénu,
- `SetupTerrainData()` - získanie `TerrainData` a nastavenie rozmerov,
- `SetupUI()` - predvyplnenie hodnôt v používateľskom rozhraní.

### `Scripts/CellularAutomata/CaDungeon.cs`

Variant CA na generovanie komnát.

- Dedí z rodičovskej triedy `CaGenerator`.

**Účel:**

- pokúša sa vytvoriť uzavreté priestory pripomínajúce komnaty,
- využíva štandardné pravidlá CA s dôrazom na súvislé oblasti.

**Verejné premenné:**

- `wallPrefab` - prefab stien,
- `floorPrefab` - prefab podlahy,
- `wallHeight` - výška stien,
- `fillProbability`, `iterations`, `birthThreshold`.

**Hlavné metódy:**

- `ReadUIInputs()`,
- `PlacePrefabs()`,
- `SetupUI()`.

---

### `Scripts/CellularAutomata/CaRoad.cs`

Variant CA na generovanie ciest.

- Dedí z rodičovskej triedy `CaGenerator`.

**Účel:**

- vytvára fragmentované cesty,
- využíva aj parameter zániku (`deathThreshold`).

**Verejné premenné:**

- `roadPrefab`,
- `fillProbability`, `iterations`, `birthThreshold`, `deathThreshold`.

**Hlavné metódy:**

- `ReadUIInputs()`,
- `PlacePrefabs()`,
- `SetupUI()`.

---

### `Scripts/CellularAutomata/CaRoom.cs`

Variant CA na generovanie miestností.

- Dedí z rodičovskej triedy `CaGenerator`.

**Účel:**

- pokúša sa vytvárať uzavreté priestory.

**Verejné premenné:**

- `roomPrefab`,
- `fillProbability`, `iterations`, `birthThreshold`.

**Hlavné metódy:**

- `ReadUIInputs()`,
- `PlacePrefabs()`,
- `SetupUI()`.

---

### `Scripts/CellularAutomata/CaTerrain.cs`

Variant CA na generovanie výškového terénu.

- Dedí z rodičovskej triedy `CaGenerator`.

**Účel:**

- transformuje CA mriežku do výškovej mapy,
- zapisuje hodnoty do terénu Unity.

**Verejné premenné:**

- `terrain`,
- `fillProbability`, `iterations`, `birthThreshold`.

**Hlavné metódy:**

- `ReadUIInputs()`,
- `PlacePrefabs()` - aplikuje výsledok CA do výškovej mapy terénu (angl. heightmap),
- `SetupTerrainData()`,
- `ResetTerrain()` - vymaže výškovú mapu terénu (angl. heightmap).

---

### `Scripts/CellularAutomata/CaTexture.cs`

Variant CA na generovanie textúr.

- Dedí z rodičovskej triedy `CaGenerator`.

**Účel:**

- vytvára dvojrozmernú textúru na základe mriežky,
- je využiteľný napríklad pre mapy alebo masky.

**Verejné premenné:**

- `targetRenderer`,
- `aliveColor`, `deadColor`,
- `fillProbability`, `iterations`, `birthThreshold`.

**Hlavné metódy:**

- `InitializeGrid()`,
- `ApplyIterations()`,
- `UpdateTexture()`.

---

### `Scripts/CellularAutomata/UI/DropdownCa.cs`

**Účel:**

- prepína medzi jednotlivými generátormi CA.

**Hlavné metódy:**

- `Dropdown(int index)` - prepne aktívny generátor podľa indexu.

---

### `Scripts/CellularAutomata/UI/GenerateCaBtn.cs`

**Účel:**

- spúšťa generovanie algoritmu CA.

**Hlavné metódy:**

- `Generate()` - spustí generovanie aktuálneho generátora CA,
- `Reset()` - vyčistí aktuálny výstup.

---

## L-systémy

### `Scripts/LSystems/LSystem.cs`

Základná trieda implementujúca expanziu L-systému.

**Účel:**

- spracováva produkčné pravidlá,
- iteratívne rozširuje vstupný reťazec (axiómu),
- generuje výsledný reťazec príkazov na interpretáciu.

**Hlavné metódy:**

- `Expand(string axiom, int iterations)` - vykoná expanziu L-systému podľa pravidiel.

---

### `Scripts/LSystems/LSystemState.cs`

Pomocná dátová štruktúra na ukladanie stavu pri vetvení.

**Účel:**

- uchováva pozíciu a rotáciu počas interpretácie L-systému,
- používa sa pri operáciách `[` a `]`.

**Verejné premenné:**

- `pos` - aktuálna pozícia,
- `rot` - aktuálna rotácia.

---

### `Scripts/LSystems/LsGenerator.cs`

Abstraktná rodičovská trieda pre všetky generátory L-systémov.

**Účel:**

- riadi celý proces generovania L-systému,
- načítava vstupy z používateľského rozhrania,
- vykonáva expanziu reťazca,
- interpretuje výsledný reťazec do trojrozmerného priestoru,
- zabezpečuje vykreslenie segmentov cez potomkov,
- poskytuje spoločné metódy pre všetky varianty L-systémov.

**Verejné premenné:**

- `axiom` - počiatočný reťazec,
- `iterations` - počet iterácií expanzie,
- `rules` - aplikované pravidlá (napr. `"F=FF+[+F-F-F]-[-F+F+F]"`),
- `step` - dĺžka jedného segmentu,
- `angle` - uhol rotácie pri príkazoch,
- `yOffset` - zvislý posun generovania,
- `axiomInput`, `iterationsInput`, `rulesInput` - vstupy používateľského rozhrania,
- `parent` - rodičovský objekt pre generované prefaby.

**Hlavné metódy:**

- `Generate()` - hlavný vstup pre generovanie,
- `ReadUIInputs()` - načítanie parametrov z používateľského rozhrania,
- `RenderLSystem(string commands)` - interpretácia reťazca na pohyb v priestore,
- `RenderSegment(Vector3 start, Vector3 end)` - abstraktná metóda na vykreslenie segmentu,
- `ClearParent()` - odstránenie predchádzajúcich objektov,
- `ClearParentContext()` - manuálne vymazanie cez kontextové menu v editore,
- `SetupUI()` - inicializácia hodnôt v používateľskom rozhraní,
- `LogLSystemMetrics()` - zber metrík (dĺžka reťazca, počet segmentov).

**Podporované príkazy:**

- `F`, `R` - pohyb dopredu a vykreslenie segmentu,
- `+` / `-` - rotácia okolo osi Y,
- `[` - uloženie stavu,
- `]` - návrat k uloženému stavu.

---

### `Scripts/LSystems/LsTreeGen.cs`

Špecializovaný generátor stromov využívajúci reprezentáciu Unity Splines.

**Účel:**

- generuje reťazec L-systému pre strom,
- používa `LsTreeRender` na vykreslenie,
- poskytuje prirodzenejší výstup než varianty založené na prefaboch.

**Verejné premenné:**

- `axiom`, `iterations`, `rules`,
- `axiomInput`, `iterationsInput`, `rulesInput`.

**Hlavné metódy:**

- `Generate()` - expanzia reťazca a spustenie vykreslenia,
- `LogLSystemMetrics()` - zber metrík,
- `SetupUI()` - inicializácia používateľského rozhrania,
- `ResetCylinderMesh()` - obnovenie assetov po generovaní.

---

### `Scripts/LSystems/LsTreeRender.cs`

Renderer stromov pomocou Unity Splines.

**Účel:**

- interpretuje reťazec L-systému do spline kriviek,
- vytvára plynulé vetvy stromu.

**Verejné premenné:**

- `step`, `angle`,
- `extrudeMesh`, `extrudeMaterial`,
- `extrudeRadius`, `segmentsPerUnit`.

**Hlavné metódy:**

- `Render(string commands)` - vykreslenie stromu z reťazca,
- `ClearTree()` - odstránenie predchádzajúceho stromu.

---

### `Scripts/LSystems/LsCave.cs`

Variant L-systému na generovanie jaskynných tunelov.

- Dedí z rodičovskej triedy `LsGenerator`.

**Účel:**

- interpretuje reťazec, ako sieť tunelových segmentov,
- vytvára trojrozmerné tunely pomocou prefabov.

**Verejné premenné:**

- `tunnelPrefab`,
- `tunnelWidth`, `tunnelHeight`.

**Hlavné metódy:**

- `RenderSegment()` - vytvára segment medzi dvoma bodmi (pozícia, rotácia, škála).

---

### `Scripts/LSystems/LsDungeon.cs`

Variant L-systému pre komnaty.

- Dedí z rodičovskej triedy `LsGenerator`.

**Účel:**

- pokúša sa vytvoriť lineárne alebo vetviace sa komnaty,
- reprezentuje segmenty, ako steny.

**Verejné premenné:**

- `wallPrefab`,
- `wallWidth`, `wallHeight`.

**Hlavné metódy:**

- `RenderSegment()` - generuje stenu medzi bodmi.

---

### `Scripts/LSystems/LsRoad.cs`

Variant L-systému na generovanie ciest.

- Dedí z rodičovskej triedy `LsGenerator`.

**Účel:**

- vytvára plynulé cestné systémy,
- prispôsobuje mierku podľa veľkosti prefabu.

**Verejné premenné:**

- `roadPrefab`,
- `roadWidth`, `roadHeight`.

**Hlavné metódy:**

- `RenderSegment()` - škáluje segment podľa vzdialenosti.

---

### `Scripts/LSystems/LsRoom.cs`

Variant L-systému na generovanie miestností.

- Dedí z rodičovskej triedy `LsGenerator`.

**Účel:**

- vytvára modulárne priestory pomocou prefabov,
- každý segment reprezentuje jednu miestnosť.

**Verejné premenné:**

- `roomPrefab`,
- `roomWidth`, `roomHeight`.

**Hlavné metódy:**

- `RenderSegment()` - umiestnenie a škálovanie miestnosti.

---

### `Scripts/LSystems/UI/DropdownLs.cs`

**Účel:**

- prepína medzi jednotlivými generátormi L-systémov.

**Hlavné metódy:**

- `Dropdown(int index)` - aktivuje vybraný generátor.

---

### `Scripts/LSystems/UI/GenerateLsBtn.cs`

**Účel:**

- spúšťa generovanie L-systému.

**Hlavné metódy:**

- `Generate()` - spustí generovanie aktuálneho generátora,
- `Reset()` - vymaže aktuálny výstup.

---

## Perlinov šum

### `Scripts/PerlinNoise/PnGenerator.cs`

**Účel:**

- základná abstraktná trieda pre všetky generátory Perlinovho šumu,
- zabezpečuje spoločnú logiku (vstupy používateľského rozhrania, seed, generovanie, čistenie scény),
- integruje meranie výkonu cez `Metrics`.

**Verejné premenné:**

- `width`, `height` - rozmery generovanej mriežky,
- `scale` - mierka šumu (priblíženie Perlinovho šumu),
- `offsetX`, `offsetY` - náhodný posun šumu,
- `heightMultiplier` - násobenie výšky / kontrastu výsledku,
- `seed` - seed pre deterministické generovanie,
- `widthInput`, `heightInput`, `seedInput`, `scaleInput`, `heightMultiplierInput` - vstupy používateľského rozhrania,
- `parent` - rodičovský objekt pre generované objekty.

**Hlavné metódy:**

- `Generate()` - hlavný vstup pre generovanie,
- `ReadUIInputs()` - načítanie parametrov z používateľského rozhrania,
- `GenerateNoise()` - abstraktná metóda implementovaná v potomkoch,
- `LogPerlinMetrics()` - výpočet štatistík (priemer, odchýlka, kontrast),
- `ClearParent()` - vymazanie generovaných objektov,
- `ClearParentContext()` - manuálne čistenie cez kontextové menu Unity,
- `SetupUI()` - inicializácia hodnôt používateľského rozhrania zo skriptu,
- `Start()` - automatické nastavenie používateľského rozhrania pri štarte.

---

### `Scripts/PerlinNoise/PnBiome.cs`

- Dedí z rodičovskej triedy `PnGenerator`.

**Účel:**

- generovanie vrstiev biómov pomocou Perlinovho šumu,
- aplikácia výsledkov na alfa mapy terénu Unity,
- rozdelenie plôch na vodu, zem a terénne oblasti.

**Verejné premenné:**

- `terrain` - objekt `Terrain`,
- `heightThreshold` - prah pre výšku (zem),
- `waterThreshold` - prah pre vodu.

**Hlavné metódy:**

- `GenerateNoise()` - generovanie alfa mapy vrstiev,
- `SetupTerrainData()` - inicializácia `TerrainData` a rozmerov.

---

### `Scripts/PerlinNoise/PnCave.cs`

- Dedí z rodičovskej triedy `PnGenerator`.

**Účel:**

- generovanie jaskýň pomocou Perlinovho šumu,
- vytvára trojrozmernú mriežku prefabov (steny / podlahy),
- používa šum, ako rozhodovací faktor pre priechodnosť buniek.

**Verejné premenné:**

- `cellSize` - veľkosť bunky,
- `wallHeight` - výška stien,
- `floorPrefab` - prefab podlahy,
- `wallPrefab` - prefab steny.

**Hlavné metódy:**

- `GenerateNoise()` - generovanie jaskynného priestoru.

---

### `Scripts/PerlinNoise/PnDungeon.cs`

- Dedí z rodičovskej triedy `PnGenerator`.

**Účel:**

- generovanie komnát pomocou Perlinovho šumu,
- rozdelenie priestoru na podlahy a steny,
- jednoduchá alternatíva ku generátoru komnát založenému na CA.

**Verejné premenné:**

- `cellSize` - veľkosť bunky,
- `wallHeight` - výška stien,
- `floorPrefab`, `wallPrefab`.

**Hlavné metódy:**

- `GenerateNoise()` - generovanie mapy komnát.

---

### `Scripts/PerlinNoise/PnRoom.cs`

- Dedí z rodičovskej triedy `PnGenerator`.

**Účel:**

- generovanie miestností a stien pomocou dvojitého Perlinovho šumu,
- kombinuje dve vrstvy šumu (miestnosť + stena),
- vytvára uzavretejšie priestorové štruktúry.

**Verejné premenné:**

- `cellSize` - veľkosť bunky,
- `wallHeight` - výška stien,
- `floorPrefab`, `wallPrefab`.

**Hlavné metódy:**

- `GenerateNoise()` - generovanie miestností na základe dvoch vrstiev šumu.

---

### `Scripts/PerlinNoise/PnTerrain.cs`

- Dedí z rodičovskej triedy `PnGenerator`.

**Účel:**

- generovanie výškových máp terénu pomocou Perlinovho šumu,
- priame zapisovanie do výškovej mapy terénu Unity (angl. heightmap),
- umožňuje procedurálne generovanie krajiny.

**Verejné premenné:**

- `terrainObject` - vizuálny objekt terénu,
- `terrain` - komponent `Terrain` v Unity.

**Hlavné metódy:**

- `GenerateNoise()` - generovanie výškovej mapy (angl. heightmap),
- `SetupTerrainData()` - nastavenie rozmerov terénu,
- `ResetTerrain()` - vynulovanie výškovej mapy (angl. heightmap).

---

### `Scripts/PerlinNoise/PnTexture.cs`

- Dedí z rodičovskej triedy `PnGenerator`.

**Účel:**

- generovanie dvojrozmernej textúry pomocou Perlinovho šumu,
- vizualizácia šumu, ako odtieňov sivej,
- využiteľné pre mapové masky alebo materiály.

**Hlavné metódy:**

- `GenerateNoise()` - aplikácia textúry na renderer,
- `GenerateTexture()` - vytvorenie `Texture2D`,
- `GenerateColor()` - prepočet hodnoty šumu na farbu.

---

### `Scripts/PerlinNoise/UI/DropdownPn.cs`

**Účel:**

- prepínanie medzi rôznymi generátormi Perlinovho šumu.

**Hlavné metódy:**

- `Dropdown(int index)` - aktivuje vybraný generátor a deaktivuje ostatné.

---

### `Scripts/PerlinNoise/UI/GeneratePnBtn.cs`

**Účel:**

- spúšťanie generovania aktuálneho variantu Perlinovho šumu,
- resetovanie generovaných objektov.

**Hlavné metódy:**

- `Generate()` - spustí generátor podľa výberu v rozbaľovacom zozname,
- `Reset()` - vyčistí aktuálny výstup generátora.

---

## WFC

### `Scripts/WFC/ModuleDef`

**Účel:**

- definuje jeden stavebný modul pre kolaps vlnovej funkcie (WFC),
- obsahuje prefab a jeho kompatibilitu v 6 smeroch (+-X, +-Y, +-Z),
- slúži, ako základný systém pre generovanie WFC.

**Verejné premenné:**

- `prefab` - prefab pre modul,
- `ports[6]` - definícia spojov (+-X, +-Y, +-Z).

**Hlavné metódy:**

- `GetPort(int dir)` - vráti typ portu pre daný smer.

**Funkcia:**

- definuje pravidlá kompatibility medzi modulmi,
- umožňuje generovanie konzistentných trojrozmerných štruktúr,
- používa sa počas tvorby tabuľky kompatibility v generátore.

---

### `Scripts/WFC/Wfc3DGenerator`

**Účel:**

- implementuje algoritmus kolapsu vlnovej funkcie v trojrozmernom priestore,
- generuje priestorovú mriežku modulov na základe pravidiel kompatibility,
- používa entropiu a prioritnú frontu na výber ďalšej bunky,
- podporuje reštart pri konflikte (prostredníctvom opätovného spustenia).

**Verejné premenné:**

- `width, height, depth` - rozmery generovanej mriežky,
- `cellSize` - veľkosť jednej bunky,
- `modules` - zoznam dostupných modulov,
- `seed` - počiatočná hodnota (angl. seed) pre deterministické generovanie,
- `maxRestarts` - počet pokusov pri konflikte,
- `widthInput, heightInput, seedInput, maxRestartsInput` - vstupy používateľského rozhrania,
- `parentForPrefabs` - rodičovský objekt pre výslednú scénu.

**Interné dáta:**

- `_generatedGrid` - finálny výsledok generovania,
- `_possibleStates` - množiny možných modulov pre každú bunku,
- `_propagationStepCount` - počet krokov propagácie,
- `_generationSuccessful` - stav generovania,
- `_allowedModuleMarks` - optimalizácia validácie kompatibility,
- `_modulesToRemove` - vyrovnávacia pamäť pre odstránenie neplatných stavov.

**Hlavné metódy:**

- `GenerateWFC()` - vstupná metóda z editora Unity cez `Inspector`,
- `Generate()` - riadi celý proces generovania,
- `TryRun()` - hlavná slučka WFC (kolaps + propagácia),
- `TryPopNextCell()` - vyberie bunku s najnižšou entropiou,
- `BuildCompatibilityTable()` - vytvorí mapu kompatibility modulov,
- `MarkAllowedModules()` - filtruje platné susedné moduly,
- `PickRandomFromSet()` - náhodný výber z možností,
- `InstantiateResult()` - vykreslenie výsledku do scény,
- `ClearPrefabs()` - vymazanie predchádzajúcej generácie,
- `GridToWorld()` - konverzia mriežky na pozíciu v scéne,
- `FirstOf()` - náhradný postup pri vyčerpaní možností.

---

# ZÁKLADNÉ SYSTÉMY

---

## `Scripts/CameraController`

**Účel:**

- dynamicky nastavuje pozíciu a vzdialenosť kamery podľa veľkosti generovanej scény,
- umožňuje prepínanie medzi dvoma kamerami (pohľad zboku / pohľad zhora),
- zabezpečuje, aby bol celý generovaný obsah viditeľný.

**Verejné premenné:**

- `mainCamera` - hlavná kamera (pohľad zboku),
- `secondaryCamera` - sekundárna kamera (pohľad zhora),
- `widthInput` - vstup šírky generovania,
- `heightInput` - vstup výšky generovania,
- `cellSizeInput` - vstup veľkosti bunky,
- `distanceMultiplier` - násobiteľ vzdialenosti kamery,
- `minDistance` - minimálna vzdialenosť kamery.

**Hlavné metódy:**

- `CameraOne()` - aktivuje hlavnú kameru,
- `CameraTwo()` - aktivuje kameru zhora,
- `LateUpdate()` - dynamicky aktualizuje pozíciu kamier.

---

### `Scripts/MainMenu/UI/MainMenu.cs`

**Účel:**

- riadi hlavné menu aplikácie,
- prepína medzi panelmi používateľského rozhrania (hlavné / sekundárne menu),
- zabezpečuje načítanie jednotlivých scén,
- umožňuje ukončenie aplikácie a export metrík.

**Serialized polia:**

- `main` - hlavný panel v menu,
- `secondary` - sekundárny panel v menu.

**Hlavné metódy:**

- `Start()` - inicializácia menu (obnova stavu po návrate do menu),
- `StartButton()` - prepne z hlavného menu na sekundárne,
- `BackButton()` - návrat zo sekundárneho menu späť,
- `QuitButton()` - ukončí aplikáciu a exportuje metriky,
- `CellularAutomata()` - načíta scénu pre CA,
- `LSystem()` - načíta scénu pre L-systémy,
- `PerlinNoise()` - načíta scénu pre Perlinov šum,
- `WaveFunctionCollapse()` - načíta scénu pre WFC.

---

## `Scripts/Metrics`

**Účel:**

- zbiera výkonnostné údaje z generovania,
- sleduje FPS, CPU, GPU, pamäť a štatistiky jednotlivých algoritmov PCG,
- exportuje výsledky do CSV na ďalšiu analýzu.

**Verejné premenné:**

- `statsText` - text používateľského rozhrania pre základné štatistiky,
- `infoText` - text používateľského rozhrania pre stav merania,
- `fileName` - názov exportovaného CSV súboru,
- `pcgResults` - zoznam nameraných výsledkov.

**Hlavné metódy:**

- `StartPcg()` - inicializuje meranie konkrétneho generovania,
- `EndPcg()` - ukončí meranie a spustí zber FPS,
- `ExportCsv()` - export všetkých výsledkov do CSV,
- `UpdateUI()` - pripojenie prvkov používateľského rozhrania,
- `LogCa()` - zaznamenanie metrík CA,
- `LogPerlin()` - zaznamenanie metrík Perlinovho šumu,
- `LogLSystem()` - zaznamenanie metrík L-systému,
- `LogWfc()` - zaznamenanie metrík WFC.

---

## `Scripts/PlaneResizer`

**Účel:**

- dynamicky mení veľkosť základnej roviny podľa parametrov generovania,
- zabezpečuje správne rozmery scény pre všetky algoritmy.

**Verejné premenné:**

- `widthInput` - vstup šírky,
- `heightInput` - vstup výšky,
- `cellSizeInput` - vstup veľkosti bunky.

**Hlavné metódy:**

- `LateUpdate()` - prepočíta a aplikuje veľkosť objektu.

---

## `Scripts/UI/UpdateMetricsUI`

**Účel:**

- prepája komponenty používateľského rozhrania so systémom `Metrics`,
- zabezpečuje zobrazenie aktuálnych štatistík v reálnom čase.

**Verejné premenné:**

- `statsText` - text pre FPS a systémové metriky,
- `infoText` - text pre stav merania.

**Hlavné metódy:**

- `Start()` - registrácia prvkov používateľského rozhrania do metódy `UpdateUI` v triede `Metrics`.

---

## `Scripts/UI/BackToMenu`

**Účel:**

- návrat do hlavného menu aplikácie,
- uchováva stav návratu medzi scénami.

**Hlavné metódy:**

- `Back()` - prepne scénu späť do menu.
