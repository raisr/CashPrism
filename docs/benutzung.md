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

Links steht die Navigation mit fünf Zielen. Auf einem schmalen Bildschirm ist
sie eingeklappt und öffnet sich über das Menüsymbol oben links. Oben rechts
wechselt ein Schalter zwischen hellem und dunklem Design, und die Schaltfläche
*Import* führt von jeder Seite direkt zum Einlesen. Unten in der Navigation
steht, welche Version von CashPrism läuft. Auf einem breiten Bildschirm klappt
das Symbol neben dem Schriftzug die Navigation zu einer schmalen Leiste aus
Symbolen ein und wieder auf.

| Ziel | Adresse | Was dort ist |
|---|---|---|
| Übersicht | `/` | Noch nichts außer einem Hinweis und dem Weg zum Import |
| Buchungen | `/bookings` | Alle gespeicherten Buchungen als Liste |
| Import | `/import` | Einen Finanzguru-Export einlesen, darunter die bisherigen Importe |
| Einstellungen | `/settings` | Alle gespeicherten Daten löschen |

Solange nichts importiert ist, zeigt *Buchungen* statt einer leeren Tabelle
einen Hinweis und eine Schaltfläche zur Seite *Import*.

## Einen Export importieren

CashPrism liest den Finanzguru-Export „Alle Buchungen“ als `.xlsx`-Datei.

1. *Import* öffnen.
2. Die Datei auf die Fläche *Finanzguru-Export hierher ziehen* ziehen — oder
   auf die Fläche klicken und die Datei wählen.
3. Warten, bis *Wird eingelesen …* verschwindet. Bei einigen tausend Buchungen
   dauert das wenige Sekunden.

Neben der Fläche steht unter *So geht’s*, wo Finanzguru den Export anbietet.

Während der Import läuft, ist die Seite abgedunkelt: Die Navigation ist nicht
erreichbar, und ein zweiter Import lässt sich nicht starten.

Dateien über 64 MB liest CashPrism nicht ein. Ein Finanzguru-Export ist weit
kleiner.

### Was die vier Zahlen bedeuten

Nach einem erfolgreichen Import meldet die Seite *Fertig! … neue Buchungen sind
da.* — oder *Fertig! Keine neuen Buchungen.*, wenn der Export nichts Neues
enthielt — und darunter vier Zahlen:

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
*Diese Datei kennen wir schon*. Dabei wird nichts gespeichert, und unter
*Bisherige Importe* erscheint kein neuer Eintrag.
CashPrism erkennt die Datei an ihrem Inhalt, nicht am Namen: Eine umbenannte
Kopie gilt als dieselbe Datei.

### Wenn eine Datei abgelehnt wird

Passt eine Datei nicht, meldet die Seite *Das ist keine Finanzguru-Datei* und
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

*Buchungen* zeigt alle gespeicherten Buchungen, zu Beginn die neuesten oben,
nach Tagen gruppiert: über den Buchungen eines Tages steht *Heute*, *Gestern*
oder das Datum mit Wochentag, etwa *Mo, 28. September 2026*.

| Spalte | Enthält |
|---|---|
| Empfänger / Absender | Wer bezahlt hat oder bezahlt wurde, davor das Symbol der Kategorie und darunter der Verwendungszweck. Lange Verwendungszwecke werden abgeschnitten; der ganze Text erscheint, wenn der Mauszeiger darauf ruht |
| Kategorie | Die Kategorie mit ihrer Farbe — siehe unten |
| Konto | Das Konto, wie Finanzguru es nennt |
| Datum | Das Buchungsdatum |
| Betrag | Der Betrag mit Vorzeichen, etwa `−12,50 €` oder `+1.850,00 €`. Euro-Beträge tragen das €-Zeichen, Beträge in einer anderen Währung deren Code, etwa `−4,00 USD`. Eingänge sind grün, Ausgänge in der normalen Schriftfarbe — das Vorzeichen sagt es auch ohne Farbe |

**Kategorien:** CashPrism fasst die Kategorien aus Finanzguru zu elf eigenen
zusammen, jede mit fester Farbe und eigenem Symbol: Wohnen, Mobilität,
Verträge & Abos, Versicherungen, Lebensmittel, Gesundheit, Shopping,
Freizeit & Essen, Einkommen, Umbuchung und Sonstiges. Restaurants und
Lieferdienste zählen zu *Freizeit & Essen*, Mobilfunk, Streaming, Cloud-Dienste,
Internet und Rundfunkbeitrag zu *Verträge & Abos*. Eine Buchung, die Finanzguru
als Umbuchung zwischen deinen eigenen Konten erkennt, ist immer *Umbuchung*.

**Sortieren:** *Empfänger / Absender*, *Datum* und *Betrag* lassen sich
sortieren. Der erste Klick auf eine dieser Überschriften sortiert absteigend,
der zweite aufsteigend, der dritte stellt die Liste wieder nach Tagen gruppiert
dar. Solange nach einer Spalte sortiert wird, fallen die Tagesüberschriften
weg.

**Blättern:** Unter der Liste steht, welche Buchungen gerade zu sehen sind, etwa
*1–25 von 6.327 Buchungen*, daneben die Seitenzahlen und die Pfeile zur
vorigen und nächsten Seite. Unter *Zeilen pro Seite* lassen sich 25, 50 oder
100 Buchungen pro Seite wählen. Alle auf einmal anzuzeigen ist bewusst nicht
vorgesehen.

**Details einer Buchung:** Ein Klick auf eine Buchung öffnet rechts ein Fenster
mit Betrag, Datum, Konto, Kategorie, Empfänger oder Absender und
Verwendungszweck. Unter dem Betrag steht, wie bezahlt wurde, etwa
*Kartenzahlung* oder *Überweisung*. Umbuchungen zwischen deinen eigenen Konten
sind dort als *Zwischen deinen Konten* markiert. Mit der Tastatur geht es
ebenso: Mit Tab zur Buchung, mit Enter öffnen. Esc, das Kreuz oben rechts oder
ein Klick neben das Fenster schließen es wieder.

**Filtern und Suchen gibt es noch nicht.** Die Liste lässt sich weder nach
Konto, Kategorie oder Zeitraum filtern noch nach einem Text durchsuchen.

## Bisherige Importe

Unter der Fläche zum Einlesen listet *Bisherige Importe* jeden Import, der
etwas eingelesen hat, die neuesten oben. Vor dem ersten Import fehlt die Liste.
Die Reihenfolge ist fest; die Spalten lassen sich nicht sortieren.

| Spalte | Enthält |
|---|---|
| Datei | Der Dateiname beim Hochladen und darunter, wann Finanzguru die Datei erzeugt hat — gelesen aus dem Namen des Tabellenblatts, *Datum unbekannt*, wenn der Name kein Datum enthielt. Ruht der Mauszeiger auf dem Namen, erscheint die Prüfsumme der Datei |
| Eingelesen | Wann importiert wurde |
| Neu | Wie viele Buchungen der Import neu gespeichert hat |
| Aktualisiert | Wie viele gespeicherte Buchungen er durch einen neueren Stand ersetzt hat |
| Zeilen gesamt | Wie viele Buchungszeilen die Datei enthielt |

Abgelehnte Dateien und Dateien, die schon einmal importiert waren, erscheinen
hier nicht: Sie haben nichts eingelesen. Einzelne Einträge lassen sich weder
löschen noch wiederholen, und ein Import lässt sich nicht rückgängig machen.

## Alle Daten löschen

Unter *Einstellungen* löscht *Alle Daten löschen* jede gespeicherte Buchung
und jeden bisherigen Import. Danach baut ein neuer Import alles wieder auf, denn
jeder Finanzguru-Export enthält deine ganze Historie. Das ist der Weg, wenn eine
neue Version von CashPrism mehr aus dem Export speichert als die alte: löschen,
dann den neuesten Export importieren.

Nach dem Klick fragt die Seite nach und nennt, wie viele Buchungen und Importe
verschwinden. Erst *Endgültig löschen* löscht, *Abbrechen* lässt alles, wie es
ist. Rückgängig machen lässt sich das Löschen nicht. Weil auch die bisherigen
Importe verschwinden, nimmt CashPrism danach auch eine Datei an, die es vorher
schon kannte. Solange nichts gespeichert ist, lässt sich die Schaltfläche nicht
anklicken.
