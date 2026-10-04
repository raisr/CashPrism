# CashPrism benutzen

Dieser Leitfaden beschreibt, was CashPrism heute kann und wie man es bedient.
Wie CashPrism gebaut ist und warum, steht in den anderen Dokumenten unter
[`docs/`](README.md).

## CashPrism starten

Ein Rechner im Heimnetz führt CashPrism aus. Solange es noch keine fertige
Programmdatei zum Herunterladen gibt, wird CashPrism aus dem Quellcode
gestartet; was dafür nötig ist, steht unter
[Getting started](../README.md#getting-started) in der README:

```bash
dotnet run --project src/CashPrism.Shell
```

Beim Start öffnet CashPrism den Browser auf diesem Rechner und schreibt ins
Konsolenfenster, unter welchen Adressen es erreichbar ist:

```
CashPrism 0.1.0

On this machine:
  http://localhost:5080

From another device in the same network:
  http://192.168.1.7:5080

Press Ctrl+C to stop.
```

Das Konsolenfenster bleibt offen, solange CashPrism läuft. `Strg+C` beendet
CashPrism. Die Daten liegen in einer Datei neben der Anwendung und bleiben
beim Beenden erhalten.

Ein anderer Port, kein Browserfenster beim Start oder dauerhafte Einstellungen:
siehe [`hosting.md`](hosting.md).

## Von einem anderen Gerät aus öffnen

Laptop, Tablet oder Handy im selben Netz öffnen die Adresse unter
*From another device in the same network* im Browser. Stehen dort mehrere
Zeilen, ist das normal — VPN- oder Docker-Adapter bringen eigene Adressen mit.
Man probiert sie der Reihe nach, bis eine antwortet.

Es gibt noch kein Passwort. Wer im Heimnetz die Adresse kennt, sieht die
Buchungen.

## Die Navigation

Links steht die Navigation mit vier Zielen. Auf einem schmalen Bildschirm ist
sie eingeklappt und öffnet sich über das Menüsymbol oben links. Oben rechts
wechselt ein Schalter zwischen hellem und dunklem Design, und die Schaltfläche
*Import* führt von jeder Seite direkt zum Einlesen. Unten in der Navigation
steht, welche Version von CashPrism läuft.

| Ziel | Adresse | Was dort ist |
|---|---|---|
| Übersicht | `/` | Noch nichts außer einem Hinweis und dem Weg zum Import |
| Buchungen | `/bookings` | Alle gespeicherten Buchungen als Liste |
| Import | `/import` | Einen Finanzguru-Export einlesen |
| Importverlauf | `/imports` | Welche Datei wann eingelesen wurde |

Solange nichts importiert ist, zeigen *Buchungen* und *Importverlauf* statt
einer leeren Tabelle einen Hinweis und eine Schaltfläche zur Seite *Import*.

## Einen Export importieren

CashPrism liest den Finanzguru-Export „Alle Buchungen“ als `.xlsx`-Datei.

1. *Import* öffnen.
2. *Export auswählen* klicken und die Datei wählen.
3. Warten, bis *Import läuft …* verschwindet. Bei einigen tausend Buchungen
   dauert das wenige Sekunden.

Während der Import läuft, liegt eine Abdeckung über der Seite: Die Navigation
ist nicht erreichbar, und ein zweiter Import lässt sich nicht starten.

Dateien über 64 MB liest CashPrism nicht ein. Ein Finanzguru-Export ist weit
kleiner.

### Was die vier Zahlen bedeuten

Nach einem erfolgreichen Import meldet die Seite *Import abgeschlossen.* und
vier Zahlen:

| Zahl | Bedeutet |
|---|---|
| gelesen | Wie viele Buchungszeilen die Datei enthielt |
| neu | Buchungen, die CashPrism zum ersten Mal gespeichert hat |
| aktualisiert | Gespeicherte Buchungen, die durch einen neueren Stand ersetzt wurden — etwa weil in Finanzguru die Kategorie oder die Gegenseite korrigiert wurde |
| unverändert | Buchungen, die so blieben, wie sie waren |

*Neu*, *aktualisiert* und *unverändert* ergeben zusammen die Zahl der gelesenen
Zeilen.

CashPrism erkennt eine Buchung an der Buchungs-ID, die Finanzguru ihr gibt.
Deshalb sammeln sich Importe an: Wer jeden Monat exportiert und importiert,
behält alle Buchungen, auch wenn ein einzelner Export nur einen Zeitraum
abdeckt. Dieselbe Buchung wird dabei nie doppelt gespeichert.

Welcher Stand einer Buchung gilt, entscheidet das Exportdatum, das Finanzguru
in den Namen des Tabellenblatts schreibt. Ein älterer Export überschreibt
nichts: Seine Buchungen zählen als *unverändert*, auch wenn sie anders aussehen
als die gespeicherten.

Meldet der Import *Unbekannte Spalten im Export*, wurde die Datei trotzdem
eingelesen. Der Hinweis bedeutet, dass Finanzguru das Exportformat geändert hat
und diese Version von CashPrism die genannten Spalten noch nicht kennt.

### Dieselbe Datei zweimal

Wird eine Datei importiert, die schon einmal eingelesen wurde, meldet CashPrism
*Diese Datei wurde bereits importiert – es hat sich nichts geändert.* Dabei
wird nichts gespeichert, und im Importverlauf erscheint kein neuer Eintrag.
CashPrism erkennt die Datei an ihrem Inhalt, nicht am Namen: Eine umbenannte
Kopie gilt als dieselbe Datei.

### Wenn eine Datei abgelehnt wird

Passt eine Datei nicht, meldet die Seite *Die Datei wurde nicht importiert.* und
nennt darunter die Gründe — etwa eine fehlende Spalte, eine leere Pflichtzelle
oder einen Wert, der kein Datum ist, jeweils mit Spalte und Zeile. Abgelehnt
wird die ganze Datei; es wird nichts davon gespeichert.

Die Seite zeigt höchstens 20 Gründe. Sind es mehr, sagt die letzte Zeile, wie
viele fehlen. Die vollständige Liste steht im Konsolenfenster des Rechners, auf
dem CashPrism läuft — auf Englisch, weil sie für eine Fehlermeldung an die
Entwickler gedacht ist.

Ist die Datei zu groß oder bricht das Hochladen ab, erscheint nur eine einzelne
Meldung ohne Liste: *Die Datei ist … MB groß* oder *Die Datei konnte nicht
gelesen werden.*

## Die Buchungsliste

*Buchungen* zeigt alle gespeicherten Buchungen, zu Beginn die neuesten oben.

| Spalte | Enthält |
|---|---|
| Datum | Das Buchungsdatum |
| Konto | Das Konto, wie Finanzguru es nennt |
| Gegenseite | Wer bezahlt hat oder bezahlt wurde |
| Verwendungszweck | Der Text der Bank. Lange Texte werden abgeschnitten; der ganze Text erscheint, wenn der Mauszeiger darauf ruht |
| Kategorie | Die Kategorie aus Finanzguru |
| Betrag | Der Betrag mit Vorzeichen und Währungscode, etwa `-12,50 EUR` oder `+1.850,00 EUR`. Eingänge sind grün, Ausgänge rot — das Vorzeichen sagt es auch ohne Farbe |

**Sortieren:** Ein Klick auf eine Spaltenüberschrift sortiert die Liste nach
dieser Spalte, ein weiterer Klick kehrt die Richtung um. Es wird immer nach
genau einer Spalte sortiert. Der Betrag wird nach seinem Wert sortiert, nicht
nach dem Text.

**Blättern:** Unter der Liste stehen die Schaltflächen zum Blättern und die
Angabe, welche Buchungen gerade zu sehen sind, etwa *1–25 von 6.327*. Unter
*Zeilen pro Seite* lassen sich 25, 50 oder 100 Buchungen pro Seite wählen. Alle
auf einmal anzuzeigen ist bewusst nicht vorgesehen.

**Filtern und Suchen gibt es noch nicht.** Die Liste lässt sich weder nach
Konto, Kategorie oder Zeitraum filtern noch nach einem Text durchsuchen.

## Der Importverlauf

*Importverlauf* listet jeden Import, der etwas eingelesen hat, die neuesten
oben. Die Reihenfolge ist fest; die Spalten lassen sich nicht sortieren.

| Spalte | Enthält |
|---|---|
| Zeitpunkt | Wann importiert wurde |
| Datei | Der Dateiname beim Hochladen. Lange Namen werden abgeschnitten; der ganze Name erscheint, wenn der Mauszeiger darauf ruht |
| Exportdatum | Wann Finanzguru die Datei erzeugt hat, gelesen aus dem Namen des Tabellenblatts. *unbekannt*, wenn der Name kein Datum enthielt |
| Prüfsumme | Die ersten zwölf Zeichen der Prüfsumme der Datei. Die vollständige erscheint, wenn der Mauszeiger darauf ruht |
| Zeilen, Neu, Aktualisiert, Unverändert | Die vier Zahlen, die der Import gemeldet hat — siehe [oben](#was-die-vier-zahlen-bedeuten) |

Abgelehnte Dateien und Dateien, die schon einmal importiert waren, erscheinen
hier nicht: Sie haben nichts eingelesen. Einträge lassen sich weder löschen
noch wiederholen, und ein Import lässt sich nicht rückgängig machen.
