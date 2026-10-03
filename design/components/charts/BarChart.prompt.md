Grouped or stacked column chart with hover tooltip.
```jsx
<BarChart labels={months} series={[{name:'Einnahmen',color:'var(--cp-income)',values:inc},{name:'Ausgaben',color:'var(--cp-expense)',values:out}]} format={eur} />
<BarChart stacked labels={months} series={cats.map(c => ({name:c.name,color:c.color,values:c.byMonth}))} />
```
