# TODO

Reference guide nasleduje za TODO listom  

* todo more inteligent planet avoidance
* projectiles - particle system yay
* while we're at particles - miner transform + actual mining (remember mining place and get dropoff from force script)
* **attack move**
* debug deceleration when in mood
* push markers out of planet radii
* multiple weapon semafor cooldown
* overshoot on turn
* selection gui (markers for different zoom levels, use unity gui ??)
* cleanup camera script (onGui)
* **attacking - circlecast stuff**
* **fog of war**
* **pathfinding**

#### Bugs

* 

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