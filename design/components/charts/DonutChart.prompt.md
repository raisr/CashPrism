Share-of-total ring with centre total; hover a segment to see its value and percentage.
```jsx
<DonutChart data={cats.map(c => ({label:c.name, value:c.total, color:c.color}))} centerLabel="Ausgaben" centerValue="2.865 €" format={eur} />
```
