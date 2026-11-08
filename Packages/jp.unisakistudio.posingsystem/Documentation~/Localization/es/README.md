# Unisaki Posing System

Sistema de poses común para Kawaii Posing, Cool Posing y otras herramientas. Utiliza Modular Avatar y NDMF para generar los menús y animaciones necesarios al subir el avatar.

Este README corresponde a v3.2.0, actualmente en desarrollo. Aún no hay fecha de lanzamiento oficial. La versión instalada aparece arriba en el Inspector. Consulta también las [novedades y cambios](CHANGELOG.md).

## Idioma de la interfaz y documentación sin conexión

Elige el idioma en «Language / 言語», arriba del Inspector. Una sola selección controla el Inspector y los menús VRChat generados. Vuelve a subir el avatar para cambiar el idioma dentro del juego.

Sin una selección explícita se usa el idioma del ordenador; si no está disponible, se utiliza inglés. Se admiten Windows, macOS y Linux. La última elección se guarda en EditorPrefs y se reutiliza en otros proyectos del mismo usuario del sistema en este ordenador. Se conserva cualquier elección explícita guardada en el avatar. No se sincroniza con otros ordenadores. El almacenamiento de las licencias no cambia.

Elige el idioma del README, historial y manual en la [lista de idiomas](../index.html) incluida. Los enlaces del Inspector al manual y los cambios también siguen esa selección. Las condiciones originales y sus traducciones de referencia siguen disponibles por separado.

## Cambio de postura en modo escritorio

Se ha reducido el retraso al aplicar la pose elegida al pasar entre de pie, agachado y sentado en escritorio. Funciona tanto en compilaciones normales como en el precálculo, sin comandos especiales ni ajustes adicionales. En VR y situaciones especiales se mantiene el procesamiento anterior. Actualiza los precálculos antiguos antes de subir el avatar.

## Instalación y actualización

Añade la herramienta con VCC o ALCOM siguiendo los archivos e instrucciones de tu compra. Este sistema se instala como dependencia de la herramienta de poses.

Los requisitos de v3.2.0 son los siguientes.

- Proyecto de avatar VRChat con Unity 2022.3
- VRChat Avatars SDK 3.10.4 o posterior
- Modular Avatar de la serie 1.x, versión 1.15.1 o posterior
- NDMF (Non-Destructive Modular Framework) de la serie 1.x, versión 1.14.0 o posterior

Si usas Kawaii Posing, actualízalo a 3.2.0 desde ALCOM / VCC y comprueba que Unisaki Posing System también sea 3.2.0. Si usabas precálculo, pulsa «Actualizar precálculo», guarda la escena y sube el avatar.

Cool Posing 1.2.0 utiliza el sistema común 3.2.0. Actualiza juntos el producto y el sistema común a las versiones correspondientes.

### Para usuarios de la versión 8bit

Las nuevas instalaciones usan el Prefab normal. Desactiva las funciones innecesarias para reducir el consumo. La selección de poses sola utiliza 8 bit de base.

Los antiguos Prefabs 8bit permanecen en «Editor/Legacy» de cada producto para conservar las referencias de avatares y preajustes existentes. No es necesario eliminarlos ni sustituirlos para seguir usándolos. El menú de Unity para añadir herramientas ya no ofrece la versión 8bit.

## Cómo se organiza la configuración

Selecciona 「可愛いポーズ」, 「カッコいいポーズ」 o el objeto equivalente dentro del avatar en la Hierarchy.

| Pestaña | Contenido |
| --- | --- |
| Funciones | Entradas del menú del juego, funciones prácticas, iconos y consumo de ExpressionParameters |
| Poses | Selección, adición y orden de poses, vista previa en Scene, detalles y ajuste de animación |
| Configuración del avatar | Correcciones de proporciones y rodillas, importación de poses originales o movimientos propios y preajustes |

Las opciones con detalles disponen de un enlace para abrir sus ajustes debajo de la entrada. La respiración también se configura en «Funciones».

Desde «Ayuda / Cambios», arriba, puedes abrir el manual, la asistencia y el historial. Puedes alternar el historial del producto y el de Unisaki Posing System.

## Funciones y consumo de ExpressionParameters

Las opciones elegidas en «Funciones > Funciones del menú» aparecen dentro del juego. Por ejemplo, «Reflejar» permite activar o desactivar el reflejo en VRChat; marcarlo en el Inspector no obliga a mantener todas las poses reflejadas.

| Función | Bits adicionales |
| --- | ---: |
| Reflejar | 1 bit |
| Sin gestos | 1 bit |
| Animar la respiración | 0 bit |
| Respiración (activar en el juego) | 1 bit |
| Altura pies | 8 bit |
| Altura suelo | 8 bit |
| Modo foto solo local | 0 bit |
| Sincronizar el Modo foto | 16 bit |
| Mantener la cabeza visible en Modo foto | 0 bit |
| Desactivar el balanceo en reposo de VRChat | 0 bit |
| Reducir deformaciones por la rotación de cabeza | 1 bit (compartido con Darse vuelta) |
| Darse vuelta | 10 bit (1 bit compartido con la corrección de deformaciones) |

Darse vuelta utiliza 1 bit para activarse, 8 bit para la inclinación de cabeza y 1 bit para el bloqueo de cuello. Si la corrección de deformaciones ya está activa, añadir Darse vuelta aumenta el consumo en 9 bit. La parte compartida no se cuenta dos veces; la cifra cambia con las selecciones.

El total mostrado corresponde a las herramientas de poses instaladas en el avatar, no al avatar completo con ropa y otros sistemas. Los ajustes comunes de varias herramientas no se cuentan dos veces. Si las poses superan el intervalo básico de numeración, su selección también necesita bits adicionales.

### Altura pies y Altura suelo

- «Altura pies»: ajusta la altura del lado de los pies conservando la posición de la cabeza. Sirve para adaptar los pies al suelo o a una cama.
- «Altura suelo»: mueve verticalmente todo el cuerpo, cabeza incluida. 50% es la altura original, 0% son 3m por debajo y 100% son 3m por encima. El valor inicial es 50%.

Ambas están en «Ajustes» dentro del juego; Altura suelo aparece después de Altura pies. Alejarla del centro desplaza el avatar arriba o abajo. Volver cerca de 50% restaura la altura original y termina la corrección. No hay un interruptor de flotación separado. La vista se corrige al comenzar a flotar y mientras se modifica la altura del suelo.

La altura de los pies ya no genera movimientos de cuerpo completo duplicados para las animaciones normales. Esto reduce los datos de animación, pero no significa que el tamaño total de todos los avatares pase a ser un tercio.

### Modo foto

1. Activa «Funciones > Funciones del menú > Modo foto».
2. En VRChat abre «Modo foto» y escoge una pose en «Elegir pose».
3. «Distancia» mueve hasta 3m hacia delante y «Dirección» gira el avatar. Con distancia mayor que 0, el avatar mira hacia ti.
4. Activa «Fijar pies» cuando sea necesario. No se impone al empezar, para permitir ajustar primero la posición. Si añadiste «Sin gestos», también puedes cambiar las manos en esa pantalla.
5. «Salir de foto» restablece distancia y dirección y libera los pies.

El movimiento y giro del Modo foto utilizan 0 bit si solo afectan a tu vista. Para mostrarlos a otras personas, activa «Funciones prácticas > Sincronizar el Modo foto». Distancia y dirección usan 16 bit en total.

Si la cabeza de tu avatar desaparece cuando está cerca, activa «Funciones prácticas > Mantener la cabeza visible en Modo foto». Solo afecta a tu propia vista durante las fotos y recupera el comportamiento normal al salir.

### Cambiar poses tumbadas al saltar

Si seleccionas poses boca arriba y boca abajo, puedes alternarlas saltando en el sitio. Con una sola orientación seleccionada, se mantiene esa postura. Para impedirlo, desactiva «Cambiar poses tumbadas al saltar» en «Funciones → Funciones prácticas» de Unity. El consumo sincronizado adicional es 0 bit. Con varias herramientas, desactivarlo en una se aplica al avatar completo.

### Darse vuelta

En las poses tumbadas compatibles, inclinar la cabeza a izquierda o derecha hace que el cuerpo ruede suavemente. No todas las poses tumbadas se vuelven compatibles automáticamente.

1. Activa «Funciones > Funciones del menú > Darse vuelta».
2. Abre los «Detalles» de la pose tumbada deseada en «Poses» y activa «Activar Darse vuelta».
3. Asigna una animación lateral a «Pose lateral para darse vuelta». La pose original y la lateral generan los giros a ambos lados. Ajusta la «Intensidad de corrección del suelo» cuando sea necesario.
4. En VRChat elige la pose compatible y activa «Darse vuelta». Mantén el cuello desbloqueado.

Al usar Darse vuelta, la inclinación de cabeza determina la posición de reproducción, por lo que el MotionTime normal queda inhabilitado. Para una animación de giro ya completa, asígnala al campo normal y deja «Pose lateral para darse vuelta» vacío. Prepara una animación con izquierda en 0, boca arriba en 0.5 y derecha en 1.

### Respiración y balanceo predeterminado

«Funciones > Funciones prácticas > Animar la respiración» mueve el pecho y otras partes al respirar. Abre los ajustes para elegir Ninguno, Bajo, Medio, Alto, Rápido o Personalizado por categoría. Rápido es una respiración acelerada; Personalizado permite asignar una animación propia.

La respiración sola utiliza 0 bit. Activa también «Funciones del menú > Respiración» para detenerla y reanudarla en VRChat. Sincronizar su activación usa 1 bit. Cambiar la intensidad no añade bits.

Cuello, hombros y brazos acompañan al pecho para limitar el movimiento de cabeza y manos. No bloquea completamente sus posiciones: elige una intensidad adecuada a las proporciones y la pose.

«Funciones prácticas > Desactivar el balanceo en reposo de VRChat» reduce el movimiento corporal de espera que VRChat añade por defecto. Está desactivado inicialmente y conserva la respiración de la herramienta y los movimientos personalizados. Funciona también sin respiración y no añade bits. No detiene oscilaciones procedentes de otras fuentes.

Con varias herramientas de poses, desactivar el balanceo estándar en cualquiera se aplica al avatar entero. Para restaurarlo, desactiva esta opción en todas y vuelve a subir el avatar.

### Reducir deformaciones por la rotación de cabeza

Úsalo si al liberar el cuello la postura se deforma porque su dirección difiere de la pose original. Está en «Funciones > Funciones prácticas» y no añade un menú propio. Con el cuello bloqueado se usa la dirección original. Las poses boca arriba están excluidas para conservar su orientación inicial hacia arriba.

### Iconos del menú

- «Generar iconos de poses»: crea imágenes del avatar en cada pose y las muestra en el menú. Facilita reconocerlas, pero alarga la compilación por la generación de imágenes.
- «Reducir el tamaño de los iconos»: baja su resolución para reducir el tamaño del avatar.

Las compilaciones Android no generan iconos. Usa «Volver a capturar iconos» después de cambiar la ropa, por ejemplo. Las imágenes de la lista se actualizan con «Actualizar miniaturas de poses». Lee la confirmación antes de empezar, porque estas operaciones tardan.

## Añadir, ordenar y previsualizar poses

Escoge una categoría a la izquierda de «Poses». La pestaña superior de lupa busca nombres en todas las categorías. «Detalles» permite ajustar animación, icono, MotionTime y otras opciones.

Añade varias animaciones arrastrándolas a la zona de adición, o selecciónalas en Project y pulsa «Importar clips seleccionados en Project». Se eliminan las referencias duplicadas al mismo clip.

Para cambiar el orden, selecciona la posición de destino en «Detalles > Orden» o usa los botones de subir y bajar. El estado abierto o cerrado de los detalles se conserva.

«Vista previa» muestra una pose en Scene sin abrir la ventana de ajuste. Al terminar vuelve el aspecto original. Esta vista sencilla no ejecuta la compilación MA o NDMF, por lo que puede diferir de la ropa añadida después o de FloorAdjuster. Comprueba también ropa y contacto con el suelo tras compilar el avatar.

## Ajuste de animación y ajuste automático de extremidades (IK)

Abre «Poses > Ajuste de animación» para corregir cuerpo y extremidades con vista previa. Al terminar guarda la animación de ajuste con los comandos de esa ventana.

En los productos y poses compatibles, el ajuste automático adapta las extremidades a las proporciones del avatar. «Activar ajuste automático de extremidades» controla el conjunto; «Activar IK para esta pose» controla cada pose. Un mensaje indica cuándo no hay ajuste automático disponible.

Para retocar manualmente el resultado automático, pulsa «Integrar IK en los ajustes». Guarda el resultado actual en una nueva animación de ajuste y desactiva IK para esa pose en ese avatar. Conserva la animación original. Requiere ajuste global y de la pose activos y configuración IK aplicable.

«Evitar la flexión inversa de rodillas» en «Configuración del avatar» permite activar su corrección. Ajusta el ángulo en «Flexión mínima de rodilla (grados)» dentro de los detalles. «Activar la corrección del avatar» requiere datos compatibles. Algunas correcciones no se pueden combinar con el ajuste automático.

## Poses originales, movimientos propios y preajustes

Se configuran en «Configuración del avatar».

- «Usar las poses originales de pie, agachado y boca abajo»: importa automáticamente las posturas del avatar.
- «Usar tus propios movimientos»: abre los ajustes de importación y asigna individualmente cada tipo, como de pie, agachado (Crouching) o boca abajo (Prone). En general, asigna un movimiento por tipo.
- «Integrar bloqueos de seguimiento con otras herramientas»: actívalo si los bloqueos no funcionan bien junto a otros sistemas.

Asignar movimientos propios es una función separada de importar automáticamente las poses originales.

Los preajustes están al final de la pestaña. Busca por nombre y usa «Aplicar este preajuste» para sustituir las poses actuales. Sirven principalmente para la configuración inicial; guarda los ajustes que quieras conservar antes de cambiarlos.

La lista solo carga nombres e información de avatares compatibles. Las animaciones de ajuste se cargan al aplicar el preajuste elegido. Ni siquiera la primera creación de la lista precarga las animaciones de todos los avatares.

La lista distingue «Compatible con el avatar», «Preajustes de usuario» y «Preajustes oficiales de Unisaki Studio». «Ocultar preajustes incluidos» es común al proyecto. La lista se actualiza al añadir, modificar o eliminar preajustes. Si no cambia, pulsa «Recargar».

## Precálculo y advertencias

Con «Usar las poses originales de pie, agachado y boca abajo» activo se detectan los movimientos de Base en cada generación. No se copian a la configuración manual: si solo usas importación automática, la lista sigue sin asignaciones. Cambiar Base o activar/desactivar la importación se aplica en la siguiente generación.

Al subir, la importación automática afecta a la copia de compilación. No modifica objetos originales de la escena, ajustes de importación, Controller fuente o movimientos. Reutiliza el precálculo si las entradas no cambian.

Las asignaciones manuales tienen prioridad por tipo, incluidas rotación, animación de ajuste y posición de reproducción. Desactivar «Usar» o retirar un movimiento asignado impide la importación automática de ese tipo. «Borrar selección manual» retira ajustes y correcciones; si la importación automática está activa, vuelve a detectarlo en la siguiente generación. Undo revierte la operación. Con varias configuraciones antiguas del mismo tipo, se prefiere la activa elegida en el Inspector.

No se borran automáticamente las entradas añadidas por la importación automática antigua porque no se distinguen de las manuales o retocadas. Usa «Borrar selección manual» solo en los tipos que quieras devolver a la detección automática. Con varios productos en el avatar, sus asignaciones manuales también se aplican a las poses originales comunes.

El precálculo reduce el tiempo de compilación reutilizando datos generados. Créalo o renuévalo con «Actualizar precálculo». La compilación normal también genera los datos necesarios; no es obligatorio precalcular manualmente cada vez.

Los cambios de configuración pueden requerir actualización, indicada en «Comprobar configuración» y las marcas de Hierarchy. Actualiza el precálculo tras cambiar poses o IK para reducir el tiempo de futuras compilaciones.

«Ajustes de visualización de advertencias» está dentro de «Comprobar configuración». Puedes ocultar tipos de avisos o marcas de Hierarchy. Ocultarlos no resuelve problemas ni actualiza datos; los errores y la generación necesaria siguen activos.

## Notas de uso

- Diseñado para seguimiento de 3 puntos y escritorio. No admite bloqueo de poses con seguimiento de cuerpo completo activo.
- Las proporciones y modificaciones pueden requerir retoques del contacto de manos/pies y la ropa.
- Capas Base especiales u otros sistemas de postura pueden producir resultados inesperados.
- Movimiento y giro del Modo foto y Altura suelo corrigen poses de la herramienta y movimientos estándar o propios sin pose seleccionada. Verifica en el cliente VRChat los movimientos reemplazados y el punto de vista.

## Condiciones de uso y asistencia

El código y los recursos de este repositorio no son de código abierto. Su uso requiere compra legítima y aceptación de las [condiciones incluidas](../../../%E3%82%86%E3%81%AB%E3%81%95%E3%81%8D%E3%83%9D%E3%83%BC%E3%82%BA%E3%82%B7%E3%82%B9%E3%83%86%E3%83%A0%E5%88%A9%E7%94%A8%E8%A6%8F%E7%B4%84%EF%BC%88%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%89%88%EF%BC%89.pdf).

- [Manual de Kawaii Posing](https://docs.unisakistudio.jp/kawaiiposing/manual/)
- [Manual de Cool Posing](https://docs.unisakistudio.jp/coolposing/manual/)
- [Asistencia](https://help.unisakistudio.jp/)

Para informar de un fallo, indica versiones del producto y Unisaki Posing System, Unity, VRChat SDK, Modular Avatar y NDMF, el avatar utilizado, los pasos para reproducirlo y el error de Console.
