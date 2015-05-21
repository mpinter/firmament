# TODO

Reference guide nasleduje za TODO listom  
Body sa skrtaju miesto mazania koli dobremu pocitu :)  
  
~~feature~~ - hotova  
`feature` - druhorada/preskocena koli deadlineu/hlupost  
**feature** - reminder sam pre seba - ffs, toto sprav uz konecne, nestihas  
  
* selecting units (box & click)
* rightclicks
* moving selected ones
* camera
* selection gui (markers for different zoom levels, use unity gui)
* cleanup camera script (onGui)


# Code Reference

Aneb ked zabudnem co som to zrobil...  
Poradie nahodne / jak prislo, use ctrl+f  
Anglicko-slovencina podla nalady, aj tak to pisem sam sebe, citat na vlastne nebezpecie.  
  
## Inheritance/Polymorphism

Pre vsetko oddedene - do startu Init(), ak nieco doplnam tak volat aj base.Init(), v Start volat Init().

#### MonoComponents

Skladuje Unity componenty, robi ich public (skaredy styl, rychlejsie kodenie).  

* renderer
* transform

## Selecting Units

Rect select and clicks separately.