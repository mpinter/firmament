# TODO

Reference guide nasleduje za TODO listom  
Body sa skrtaju miesto mazania koli dobremu pocitu :)  
  
~~feature~~ - hotova  
`feature` - druhorada/preskocena koli deadlineu/hlupost  
**feature** - major/time consuming features

* overshoot on turn
* turn speed zavisli od radiusu
* create marker as child for every unit
* move in unitscript  
* ~~selecting units (box & click)~~
* rightclicks
* moving selected units
* camera
* selection gui (markers for different zoom levels, use unity gui ??)
* cleanup camera script (onGui)
* **attacking - circlecast stuff**
* **fog of war**
* **pathfinding**


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

Rect select and clicks separately. TODO document moar

## Marker

Pamata si vsetky unity co su na neho smerovane