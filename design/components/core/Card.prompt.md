The surface for every block: white, 1px line, 14px radius, soft shadow (border only in dark).
```jsx
<Card title="Ausgaben nach Kategorie" subtitle="September 2026" action={<Button variant="ghost" size="sm">Details</Button>}>
  <DonutChart data={cats} />
</Card>
<Card title="Letzte Buchungen" padded={false}><TransactionList items={tx} /></Card>
```
