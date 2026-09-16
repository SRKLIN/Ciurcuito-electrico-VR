# Guion del guia virtual

Veinte lineas para grabar. Cada una es un `Parlamento` del componente
`GuiaVirtual`, en el GameObject **Guia** de la escena.

> Este archivo esta generado a partir de los textos que estan puestos en la
> escena. Si se cambia una frase en el Inspector, volver a generarlo para que
> el guion grabado y el subtitulo no se separen.

## Como ponerlas en su sitio

1. Seleccionar **Guia** en la jerarquia.
2. En el componente `Guia Virtual`, abrir el bloque que corresponda.
3. Arrastrar el archivo de audio a la ranura **Voz** de esa linea.

Lo que no tenga audio se muestra igual como texto, con una duracion calculada
por el largo de la frase. Con audio puesto manda el clip: la linea dura
exactamente lo que dura la grabacion. **No hay que tocar codigo ni ajustar
duraciones a mano.**

El texto se sigue viendo en el bocadillo aunque haya voz, como subtitulo.

---

## Bienvenida

**Donde:** En el cuarto de entrada, parado, mirando al jugador.  
**Campo en el Inspector:** `bienvenida`  
**Nota:** Se dispara cuando el jugador se acerca a 2.2 m, o si lo toca antes.

### bienvenida-01 &nbsp; `guia-bienvenida-01.wav`

> Hola. Bienvenido al laboratorio de circuitos. Yo soy tu guía: dejame mostrarte cómo se trabaja acá.

### bienvenida-02 &nbsp; `guia-bienvenida-02.wav`

> Lo primero son las manos. Apuntá a una pieza con el mando y apretá el botón lateral, el del dedo medio, para agarrarla. Con las manos sueltas, acercá la palma y cerrá los dedos.

### bienvenida-03 &nbsp; `guia-bienvenida-03.wav`

> Para soltarla, abrí la mano o soltá el botón. Si algo se te cae no pasa nada: siempre hay repuestos en las estanterías.

### bienvenida-04 &nbsp; `guia-bienvenida-04.wav`

> Para caminar usá la palanca izquierda. Con la derecha girás de a saltos. Probá un momento, y cuando estés listo, seguime.

---

## De camino a la practica

**Donde:** Caminando, del cuarto a la zona de practica.  
**Campo en el Inspector:** `deCamino`  
**Nota:** Suena mientras se mueve. Si el jugador se queda atras, el guia frena pero sigue hablando.

### camino-01 &nbsp; `guia-camino-01.wav`

> La puerta se abre sola cuando alguien se acerca. No hay que tocar nada.

### camino-02 &nbsp; `guia-camino-02.wav`

> Este es el laboratorio. A la izquierda, sobre el piso gris, está la zona de práctica. A la derecha, sobre el piso color arena, la zona de retos.

### camino-03 &nbsp; `guia-camino-03.wav`

> Vamos primero a la práctica. Ahí no se rompe nada importante y podés probar todo lo que quieras.

---

## En la zona de practica

**Donde:** Parado entre Mesa Armado y Mesa Circuito.  
**Campo en el Inspector:** `enPractica`  
**Nota:** Es el bloque mas largo: seis lineas.

### practica-01 &nbsp; `guia-practica-01.wav`

> Estas son las mesas de armado. Cada hueco marcado en la tapa espera una pieza: la pila, el interruptor, la resistencia y la bombilla.

### practica-02 &nbsp; `guia-practica-02.wav`

> Las piezas están en la estantería de al lado. Agarrá una, acercala al hueco que le toca y soltala: se encastra sola.

### practica-03 &nbsp; `guia-practica-03.wav`

> Cuando el circuito queda completo, la bombilla enciende. La pizarra de atrás muestra el mismo circuito en esquema, y los puntos naranjas son la corriente circulando.

### practica-04 &nbsp; `guia-practica-04.wav`

> En esta mesa el voltaje es fijo y vos cambiás la resistencia. En la de al lado es al revés: la resistencia es fija y subís el voltaje con la fuente. Son las dos caras de la ley de Ohm.

### practica-05 &nbsp; `guia-practica-05.wav`

> Ojo con la fuente: pasando los doce voltios la bombilla se quema. Si pasa, tocala con el dedo y se cambia por una nueva.

### practica-06 &nbsp; `guia-practica-06.wav`

> Al fondo hay una mesa más, la de electrónica, con el condensador, el diodo y el motor. Cuando termines acá, date una vuelta por ahí.

---

## De camino a los retos

**Donde:** Caminando, de la practica a la mesa de retos.  
**Campo en el Inspector:** `deCaminoARetos`  
**Nota:** Arranca cuando el jugador se queda 3 s cerca del guia (o lo toca).

### camino-retos-01 &nbsp; `guia-camino-retos-01.wav`

> Vamos para el otro lado, al piso color arena.

### camino-retos-02 &nbsp; `guia-camino-retos-02.wav`

> Te aviso antes de llegar: allá las mesas no te ayudan. No hay carteles en los huecos, y los huecos aceptan cualquier pieza.

---

## En la zona de retos

**Donde:** Parado al costado de Mesa Reto.  
**Campo en el Inspector:** `enRetos`  
**Nota:** Ultimo bloque. Al terminar se queda quieto para siempre.

### retos-01 &nbsp; `guia-retos-01.wav`

> Esta es la mesa de retos. La pizarra te va a pedir tres cosas, una por vez, y las resolvés con lo que hay en la estantería.

### retos-02 &nbsp; `guia-retos-02.wav`

> Podés poner las piezas en cualquier orden: en un circuito en serie la corriente es la misma en todo el lazo, así que da igual por dónde empieces.

### retos-03 &nbsp; `guia-retos-03.wav`

> Lo único que no podés es repetir familia. Dos pilas o dos resistencias marcan error, porque entonces algo está faltando.

### retos-04 &nbsp; `guia-retos-04.wav`

> El primer reto es encender la bombilla. El segundo pide brillo alto, y hay una sola combinación que llega. El tercero es el bueno: encendida, pero muy baja. Ese no se resuelve poniendo la pila más grande.

### retos-05 &nbsp; `guia-retos-05.wav`

> Cuando termines, atrás tenés el nivel dos: cuatro huecos y siete piezas, así que hay que elegir qué dejar afuera. Ahí hay trampa, pero no te digo cuál.

---

## Carteles sueltos

No son parlamentos: es texto fijo que queda en el cartel mientras el guia
espera. No llevan audio.

| Campo | Texto |
|---|---|
| `textoLlamada` | Acercate y te explico |
| `invitacionRetos` | Acercate unos segundos y te llevo a la zona de retos |
| `textoFinal` | Me quedo por acá. Suerte. |

Al cartel de `invitacionRetos` se le van agregando puntos suspensivos mientras
corren los 3 segundos de espera, para que se vea que la cuenta avanza.

---

**Total: 20 lineas para grabar.**
