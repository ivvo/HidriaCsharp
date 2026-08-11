# Dokumentacija razlik — `Form_03` (PHEVVision ↔ Alutec AUDI)

**Projekta:**
- **PHEV** = `D:\C#\Ivo\Working\PHEVVision\HidriaVisionWinForms`
- **Alutec** = `D:\C#\Alutec AUDI\HidriaVisionWinForms`

**Skupna osnova:** oba git commit `f9a93fc` (`master`). Razlike so necommitane spremembe delovnega drevesa.

**Obseg:** razlikujejo se **samo 4 datoteke**, vse v `HidriaVision\Form_03\`. Vsi ostali projekti, forme, `.sln` in `.csproj` so bitno enaki. PHEV nima `Form_03B`, `Form_13` ali `VidiSubTaskManagment`.

---

## 1. `Form_03.Designer.cs` — nova kontrola za prikaz slike

**Razlika:** Alutec doda `PictureBox` za prikaz slike v levi panel prikaznega `SplitContainer`-ja; PHEV te kontrole nima.

| | PHEV | Alutec |
|---|---|---|
| `pictureBoxDisplay` | ❌ ni | ✅ dodan |

Dodano v Alutec:
```csharp
private System.Windows.Forms.PictureBox pictureBoxDisplay;
// ...
this.pictureBoxDisplay.BackColor = System.Drawing.Color.Black;
this.pictureBoxDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
this.pictureBoxDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
this.pictureBoxDisplay.TabStop = false;
this.splitContainerDisplay.Panel1.Controls.Add(this.pictureBoxDisplay);
```
**Vpliv:** Alutec ima namensko površino (`Panel1`, črno ozadje, Zoom) za prikaz zajete slike; PHEV ima ta panel prazen.

---

## 2. `Form_03.cs` — pot za shranjevanje slik + čiščenje LED kode

**a) Pot slikovnega loggerja (funkcionalna razlika!):**

| | PHEV | Alutec |
|---|---|---|
| `ImageLogger` cilj | `./Log/ImageLog` (relativno na app) | `C:\Slike` (absolutno, trdo kodirano) |

```csharp
// PHEV:
ImageLogger.Value = new FileImageLogger("./Log/ImageLog", "Station06", 0, 0, ImageLoggerErrorHandler);
// Alutec:
ImageLogger.Value = new FileImageLogger(@"C:\Slike", "Station06", 0, 0, ImageLoggerErrorHandler);
```
**Vpliv:** Alutec shranjuje slike na fiksno lokacijo `C:\Slike\...` (mora obstajati na disku!); PHEV v mapo znotraj aplikacije. `MaxNumberOfImages = 200*2` ostane enako.

**b) LED-controller init blok:** PHEV še vsebuje velik **zakomentiran** blok inicializacije `RGBWController` (odpiranje serijskega porta, intenzitete, SurfaceFx); Alutec ta blok v celoti odstrani (nadomesti s prazno vrstico). Kozmetično — koda je bila že prej neaktivna.

---

## 3. `Form_03.MainOperation.cs` — sanitizacija imena DMC + shranjevanje slik

**a) Čiščenje DMC kode v ime datoteke (funkcionalna razlika):**

| | PHEV | Alutec |
|---|---|---|
| Metoda | `.Replace('*','_')` (samo `*` in `/`) | `Regex.Replace(DMC, "[^a-zA-Z0-9]", "_")` |

```csharp
// PHEV:
string DMC_CODE_NAME = DMC_codeRequest.Value.Replace('*', '_');   // + Replace('/','_')

// Alutec:
string DMC_CODE_NAME = Regex.Replace(DMC_codeRequest.Value, @"[^a-zA-Z0-9]", "_");
```
**Vpliv:** Alutec je robustnejši — zamenja **vse** ne-alfanumerične znake (varno ime datoteke za katerikoli DMC); PHEV zamenja le `*` (in `/`), ostali posebni znaki bi lahko povzročili neveljavno pot.

**b) Pot slikovnega rezultata:**
```csharp
// PHEV:
CommonLogResults.Image1 = new ImageSource($"./Log/ImageLog/Station06/Type000/P000/OK/{ConstructedImageSFX1Name}");
// Alutec:
CommonLogResults.Image1 = new ImageSource($@"C:\Slike\Station06\Type000\P000\OK\{ConstructedImageSFX1Name}");
```
Skladno s spremembo poti loggerja (točka 2a).

**c) Dodano lovljenje napak (Alutec):** dodani `using System.Drawing; System.IO; System.Text.RegularExpressions;` in `try/catch` z log-vnosi:
```csharp
Logger.AddEntry(LoggingLevel.Error, $"Station03: Napaka pri shranjevanju zadnje slike: {e.Message}");
Logger.AddEntry(LoggingLevel.Error, $"Station03: Napaka pri prikazu slike: {e.Message}");
```
**Vpliv:** Alutec robustneje obravnava napake pri shranjevanju/prikazu slike (verjetno povezano z novim `pictureBoxDisplay` iz točke 1).

---

## 4. `Form_03.Camera01.cs` — trivialno

Alutec ima eno dodatno **zakomentirano** vrstico; brez funkcionalnega vpliva.
```csharp
//Camera01SFXImages.Value = new Bitmap(e.GrabbedImage);
```

---

## Povzetek

| Področje | PHEVVision | Alutec AUDI | Tip |
|---|---|---|---|
| Prikaz slike (`pictureBoxDisplay`) | ni | **dodan** | funkcionalno (UI) |
| Shranjevanje slik | `./Log/ImageLog` | **`C:\Slike`** | funkcionalno |
| Sanitizacija DMC imena | preprost `Replace` | **Regex** (vsi znaki) | funkcionalno (robustnost) |
| Obravnava napak slik | ni | **try/catch + log** | funkcionalno (robustnost) |
| LED init blok | zakomentiran (ostane) | odstranjen | kozmetično |
| Camera01 komentar | — | +1 komentar | kozmetično |

**Zaključek:** Alutec AUDI je v `Form_03` **novejši/bolj dodelan** (namenski prikaz slike, robustna sanitizacija DMC, fiksna lokacija `C:\Slike`, lovljenje napak). PHEVVision je bližje osnovnemu commitu. Edina konceptualna odločitev za sinhronizacijo: **kam shranjevati slike** — relativno (`./Log/ImageLog`, prenosljivo) vs. absolutno (`C:\Slike`, fiksno).
