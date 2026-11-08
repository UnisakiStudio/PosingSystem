# Historial de cambios

## [3.2.0] - Sin publicar

### Añadido

- Inspector, operaciones de edición, diálogos de instalación, nombres de poses estándar y menús VRChat generados disponibles en 30 idiomas.
- Selección de idioma unificada para Inspector y menú del juego. Inicialmente usa el idioma del ordenador y recuerda la última elección en otros proyectos del mismo usuario del sistema.
- Apertura del README, historial y manual sin conexión en el idioma elegido.
- Nombres de las 128 poses estándar y descripciones de funciones preparados con terminología coherente con la existente.

### Compatibilidad

- Se conservan configuración del avatar, nombres propios, referencias a recursos, identificadores de parámetros y almacenamiento de licencias.

## [3.1.2] - 2026-11-09

- Corregido el rostro que permanecía mirando arriba al levantarse desde boca abajo con «Reducir deformaciones por la rotación de cabeza» activo.
- El bloqueo de cabeza se puede cambiar en «Ajustes → Bloqueo automático de cabeza boca arriba» del menú EX incluso con Altura pies desactivada. Las entradas perdidas en 3.1.0 o 3.1.1 vuelven tras actualizar y compilar.
- «Actualizar precálculo» funciona también con el objeto principal del avatar desactivado en Unity.
- Corregida la imposibilidad ocasional de generar poses tras actualizar cuando quedaba un Controller común antiguo en Assets. Se conservan también las modificaciones personales del Controller.
- Si la herramienta o un padre desactivado impide usar el Animator común, se muestra el nombre correspondiente. «Comprobar configuración → Seleccionar objeto» permite localizarlo.
- Cuando el precálculo indica cambios, sus detalles permiten consultar qué elementos difieren del último guardado.
- Al detenerse una compilación por errores de scripts (PS050), se muestran hasta 5 errores originales detectados por Unity. La función de consulta abre un borrador con versiones de Unity y herramientas compatibles y número de error, que se puede revisar, editar y enviar.

Si usabas precálculo, pulsa «Actualizar precálculo» tras actualizar, guarda la escena y vuelve a compilar y subir el avatar.

## [3.1.1] - 2026-11-08

- Altura suelo y Darse vuelta activados aparecen en Ajustes del menú EX de VRChat incluso con Reflejar desactivado.
- Se conservan los movimientos correctos para «Piernas extendidas», «Relajado» y «Una rodilla arriba» tras ordenar, eliminar o renombrar en versiones antiguas, incluso con Unpack completo si se identifica el origen. Se mantienen poses propias, Darse vuelta desactivado intencionadamente y animaciones laterales reemplazadas.
- Corregidos el breve paso boca abajo al elegir avatar y el breve paso boca arriba al saltar con solo poses boca abajo seleccionadas. El cambio con salto se desactiva en «Funciones → Funciones prácticas → Cambiar poses tumbadas al saltar» y está activo inicialmente.
- Si se pierde una referencia recuperable del Animator Controller común, aparece un aviso y se reconstruye desde los ajustes. Se mantienen referencias compartidas entre avatares.
- Los errores de compilación detienen el proceso al principio indicando la causa. Se suspende la limpieza automática de Controllers mientras no se puedan verificar sus referencias.
- Al añadir la herramienta se prioriza el Prefab oficial aunque haya otro con el mismo nombre. Las diferencias solo de saltos de línea del Controller ya no impiden el precálculo.
- Los Write Defaults de capas FX añadidas, como Altura suelo, siguen los ajustes del avatar.

Si usabas precálculo, actualízalo tras la actualización desde la herramienta de la escena, guarda y vuelve a compilar y subir el avatar.

## [3.1.0] - 2026-11-07

- Ajuste IK de extremidades según proporciones para poses compatibles, correcciones del avatar y prevención de flexión inversa de rodillas.
- Posición y dirección configurables en Modo foto, con sincronización y visibilidad de la propia cabeza opcionales.
- Altura suelo mueve todo el cuerpo hasta 3m arriba o abajo, separadamente de Altura pies.
- Añadidos respiración, eliminación del balanceo estándar y corrección de deformaciones por dirección del cuello.
- Funciones y consumo de parámetros visibles en Inspector; búsqueda, importación múltiple, orden y vista previa sencilla en Scene.
- Cambios de pie, agachado y sentado acelerados en escritorio.
- Mejoradas advertencias tras cambios de precálculo, desplazamiento del Inspector y pausas de limpieza.
- Corregidos avisos falsos y regeneración al subir con importación automática y precálculo sin cambios. Los resultados se aplican al avatar generado sin modificar la importación del original.
- Darse vuelta con inclinación de cabeza y movimientos compatibles por pose añadidos.
- Mejorados transferencia de preajustes antiguos, importación propia y administración de recursos generados.

## [3.1.0-beta.2] - Beta abierta (2026-11-05)

- Longitud fija y diferencias de altura (Y) y profundidad (Z) entre Head y Viewpoint añadidas como referencias de longitud de posición IK.
- Avisos del Inspector también a la izquierda de nombres en Hierarchy: precálculo ausente o modificado y avisos de importación o preajustes.
- Corregidas pausas largas de Unity cuando la limpieza automática al abrir escenas cargaba Controllers generados de otras escenas.
- Corregida pérdida del historial de precálculo al cambiar poses o funciones. Ahora se indica modificación posterior, no ausencia de precálculo, con aviso también en Hierarchy.
- Comparaciones de precálculo compartidas entre Inspector y Hierarchy para aligerar el desplazamiento sin cambios de configuración o recursos. La compilación sigue comparando cada vez.

## [3.1.0-beta.1] - Beta abierta (2026-11-04)

- Cambio de postura normal en escritorio mediante una sola evaluación, tanto en compilación normal como precálculo; VR y casos especiales conservan la ruta anterior.
- Precálculos alpha.3 o anteriores reconocidos como pendientes de actualizar.
- Vista conservada al cambiar Altura suelo en Modo foto y restaurada a la última altura al terminar.

- Importación automática limitada a detectar en generación, sin añadir ni modificar asignaciones manuales durante precálculo o compilación.
- La regeneración refleja activación automática, cambios de Base y retirada de asignaciones. Conserva ajustes compartidos entre productos y devuelve movimientos antiguos importados a los estándar.
- Controllers de precálculo en un archivo por avatar y GUID de escena. Referencias compartidas por duplicados, otras escenas o Prefabs separadas antes de actualizar.
- Uso de recursos temporales NDMF durante compilación sin aumentar archivos de edición generados.
- Datos generados comprobados al abrir y guardar. Se protegen cambios sin guardar, Undo/Redo y referencias externas; solo archivos sin uso y propietario claro van a la papelera.
- Rotación, ajuste de animación y posición de reproducción aplicados a movimientos importados; rotación, RootT/RootQ y ajuste Humanoid unificados con las poses normales.
- Ajustes de BlendTree anidados duplicados para su destino, conservando el movimiento fuente y las mezclas de caminar.
- La reproducción «solo de pie» se convierte en pose estática a la frecuencia original sin detener la parte de caminar.

## [3.1.0-alpha.2] - Prueba cerrada (2026-10-26)

### Correcciones y cambios

- Modo foto y Altura suelo también corrigen movimientos estándar o propios sin pose seleccionada.
- Altura suelo ampliada a 3m arriba y abajo: 50% original e inicial, 0% 3m debajo y 100% 3m encima.
- Corregida modificación del BlendTree fuente al reemplazar «solo de pie».
- Ajuste manual en poses sin reglas IK, evitando avisos de generación innecesarios.
- Corregidos borradores mezclados entre poses homónimas y cambios de movimiento ignorados dentro del mismo recurso.
- Corregida selección inicial de otra pose por el número de una pose inicial desactivada.
- Explicación en vez de excepción al importar AnimatorOverrideController no compatibles.
- Darse vuelta desactivado explícitamente se conserva al aplicar preajustes antiguos.
- Iconos más legibles en el tema claro del Editor.

Continúan las comprobaciones de vista y movimientos estándar reemplazados dentro del cliente VRChat.

## [3.1.0-alpha.1] - Antes de publicar

### Añadido

- Activación por función y consumo de ExpressionParameters añadidos.
- Modo foto con avance y giro del avatar añadido.
- Sincronización opcional: 0 bit local, 16 bit sincronizados.
- Opción para mantener visible la cabeza propia en Modo foto.
- Altura suelo desplaza todo el cuerpo y cabeza 3m arriba o abajo: 50% original e inicial, 0% 3m debajo y 100% 3m encima.
- Darse vuelta con inclinación de cabeza y animaciones por pose añadidos.
- Respiración con Bajo, Medio, Alto, Rápido y Personalizado.
- Movimiento y menú de respiración independientes: 0 bit movimiento solo, 1 bit activación sincronizada.
- Supresión del balanceo estándar de espera de VRChat.
- Corrección de deformaciones por dirección del cuello.
- Ajuste IK según proporciones para poses compatibles y activación global o por pose.
- «Integrar IK en los ajustes» guarda el resultado en animación de ajuste.
- Correcciones del avatar, prevención de flexión inversa y ángulo mínimo de rodillas.
- Vista previa sencilla de poses en Scene.
- Pestañas de categoría, búsqueda transversal, importación múltiple y elección de orden.
- Historial del producto y sistema común en Inspector.

### Cambios

- Preajustes más rápidos, con menos espera al añadir la herramienta o abrir la lista.
- Lista actualizada al añadir, editar o eliminar preajustes.
- Inspector en 3 pestañas: Funciones, Poses y Configuración del avatar.
- Explicaciones e iconos añadidos y apertura de detalles unificada.
- Respiración trasladada a Funciones y preajustes al final de Configuración del avatar.
- Importación de movimientos propios separada de detección automática de poses originales.
- Menos animaciones de cuerpo entero duplicadas para Altura pies.
- Actualización de miniaturas con botón y confirmación.
- Preferencias de avisos reunidas en Comprobar configuración.
- Nuevos Prefabs 8bit retirados a favor del normal configurable, conservando referencias existentes.
- VRChat Avatars SDK mínimo actualizado a 3.10.4.

### Correcciones

- Modo foto y Altura suelo aplicados también a movimientos estándar o propios sin selección.
- BlendTree fuente no se modifica al reemplazar «solo de pie».
- Ajustes manuales sin IK válida y avisos IK innecesarios suprimidos.
- Corregidos ajustes mezclados entre poses homónimas y cambios de movimiento dentro de un recurso no regenerados.
- Números reutilizados de poses iniciales desactivadas ya no provocan selección inicial incorrecta.
- Aviso en vez de excepción ante AnimatorOverrideController no compatible al importar poses originales.
- Darse vuelta explícitamente desactivado conservado en preajustes antiguos.
- Fondo de iconos mejorado para distinguirlos en tema claro.

- Corregidos iconos ausentes de Darse vuelta, Altura suelo y Modo foto al añadir el avatar.
- Corregida pérdida de Darse vuelta al aplicar preajustes antiguos.
- La escritura de nombres no salta a otra pose al abrir sus detalles.
- Corregidos detalles cerrados al ordenar y Undo/Redo de ajustes.
- Corregidos avatar original superpuesto y posición de inserción duplicada en vista previa.
- Corregidos errores de vista previa con componentes ausentes.
- Corregidas posiciones y orientaciones IK por diferencias de ejes y escalas óseas.
- Avisos y precálculo actualizados inmediatamente tras cambios.
- Resultados antiguos no reutilizados tras cambios IK.
- Corregidos fallos de generación de funciones adicionales en algunas configuraciones.

## [3.0.16] - 2026-09-14
- NDMF mínimo actualizado a 1.14.0 por el uso de la API `VirtualAnimatorController.SetParameter`.

## [3.0.15] - 2026-09-13
- Corregidos grandes cambios de posición del cuerpo y flexión de rodillas al alternar el bloqueo de cuello boca arriba con escala Hips modificada y FloorAdjuster.

## [3.0.14] - 2026-09-09
- Corregida desaparición de Sin gestos desde la segunda herramienta en un mismo avatar.
- Corregida vuelta de USSPS_Locomotion a Standing pese a elegir poses como SleepDown.
- Corregidos desajustes de altura y X/Z con FloorAdjuster, respetando el orden MA y AAO.
- Restauración automática de PosingSystem_Locomotion estándar si se borra el AnimatorController precalculado en Generated.
- Corregido error de compilación al eliminar todas las poses de pie.
- Preajustes ordenados por nombre de avatar.

## [3.0.13] - 2026-09-04
- Corregida autotransición de pie que desestabilizaba la recuperación al levantarse justo después de empezar a agacharse en VR.

## [3.0.12] - 2026-09-04
- Corregida detección falsa de cambios al compilar con NDMF una herramienta sin precálculo.
- Corregido `AddAssetToSameFile` al añadir recursos generados a un AnimatorController temporal durante NDMF.

## [3.0.11] - 2026-08-31
- Corregido fallo por Scene inválida del avatar de precálculo con NDMF 1.14.7 o posterior.
- Corregida `MissingReferenceException` cuando la pantalla de errores NDMF referenciaba un LocalizationAsset destruido.

## [3.0.10] - 2026-08-29
- Carga real de preajustes retrasada hasta abrir su selector, con reutilización de caché si no cambia el índice GUID.
- Iconos de avisos y errores de Hierarchy reevaluados solo tras cambios y búsqueda de recursos del menú compartida en caché. Menos bloqueos cada pocos segundos con muchas herramientas en la escena.
- Corregida `ArgumentException` al registrar parámetros duplicados en precálculos de varias herramientas del mismo avatar.

## [3.0.9] - 2026-08-26
- Evitada NullReferenceException si no se obtiene VRCAvatarDescriptor desde PosingSystem, indicando los registros de compilación necesarios.
- Corregido `Avatar is null` al reutilizar una vista previa de un Avatar temporal creado y borrado por NDMF en precálculos o compilaciones posteriores. Conversión separada de caché de vista previa y nombres temporales protegidos contra colisiones entre vistas.
- Corregidas referencias de vista previa de 「可愛いポーズ」 no guardadas y multiplicación de avatares ocultos. Vistas excluidas del guardado y antiguas recuperadas al cargar escena, entrar en Play o recargar scripts.

## [3.0.8] - 2026-08-18
- Corregidos Unity Assertion y posible corrupción de poses al recalcular altura con FloorAdjuster o similares en movimientos precalculados.
- Medición de altura protegida para no interferir con el avatar original ni AnimationMode de otras herramientas.

## [3.0.7] - 2026-08-12
- Corregidas vista desplazada y rodillas anómalas boca arriba con FloorAdjuster y otras herramientas de altura.
- Corregido error con PosingSystem fuera del avatar (gracias a PancakeTorto1se).
- Corregidos errores por continuar tras fallar la subida del avatar.
- Corregidas falsas modificaciones justo después del precálculo.
- Mejorada importación automática de locomoción.
- Avisos para avatares con ciertos rigs especiales.
- Confirmación de cambios y reflejo de valores en la ventana de ajuste (gracias a kraihd).
- Indicaciones ante errores de otras herramientas y controles en puntos propensos a fallos.

## [3.0.6] - 2026-03-17
- Corregidas poses muy deformadas en avatares con control de postura propio.

## [3.0.5] - 2026-01-31
- Corregidos cierres de Unity con ciertos ExpressionMenus.
- Corregidos problemas del avatar con Prefab de integración ごろ寝EX (gracias a あずき).
- Corregidas rotaciones erróneas al fusionar animaciones de ajuste.

## [3.0.4] - 2026-01-11
- Mejorado parcialmente el comportamiento de avatares en Prefabs.
- Eliminación de licencia añadida.
- Corregidos problemas de cuello en muchas poses.
- Apertura del Inspector algo más ligera.
- Generación de iconos también durante precálculo y cambio del texto de actualización del avatar.
- Avisos ocultos si nunca hubo precálculo, dado que muchos usuarios parecen no usarlo.
- Recursos innecesarios eliminados.
- Corregida imposibilidad de ajustar dedos.

## [3.0.3] - 2026-01-04
- Corregida aplicación de otro preajuste que el elegido (gracias a あずき).

## [3.0.2] - 2026-01-02
- Compatibilidad con tema oscuro de Unity.
- Corregidas deformaciones tras girar en el ajuste de animación.
- Iconos generados redimensionados y comprimidos, con opción aún más pequeña.
- Restauradas integración de seguimiento e importación automática, retiradas en 3.0.0.

## [3.0.1] - 2025-12-29
- Corregida configuración de movimientos predeterminados.
- Corregidas posturas que se deformaban solas.

## [3.0.0] - 2025-12-28
- Precálculo añadido.
- Ajuste de animación añadido.
- Preajustes añadidos.

## [2.2.5] - 2025-05-08
- Resueltos problemas de compatibilidad con ciertos sistemas.

## [2.2.4] - 2025-04-04
- Corregido bloqueo del cambio de poses con ciertos avatares y versiones de ModularAvatar.

## [2.2.3] - 2025-03-13
- Corregidos movimientos extraños en avatares como el salto de 森羅.
- Corregido valor inicial a veces erróneo de Sin gestos.

## [2.2.2] - 2025-03-06
- Corregidos errores en algunos entornos.

## [2.2.1] - 2025-02-05
- Corregido cierre de Unity.
- Versión añadida al nombre en NDMF Console.

## [2.2.0] - 2025-02-02
- Añadida opción Sin gestos para usar las manos de la animación.

## [2.1.6] - 2025-02-02
- Corregidas poses deformadas con ciertas herramientas.
- Corregidos errores dependientes de ajustes.

## [2.1.5] - 2024-12-10
- Conflictos resueltos con ciertos sistemas.
- Botón para eliminar versión Unitypackage añadido.

## [2.1.4] - 2024-12-08
- Corregidos errores en ciertos avatares.
- Corregido bloqueo de pies inutilizable por conflictos con sistemas o avatares.
- Corregida vuelta a posturas estándar VRC en escritorio.

## [2.1.3] - 2024-12-01
- Corregido cambio imposible con avatares o sistemas que usan VRMode con tipo incorrecto.
- Aviso en NDMF-Console de conflictos por tipos de parámetros diferentes.

## [2.1.2] - 2024-12-01
- Explicación en NDMF-Console cuando fallos de otras herramientas provocan errores en Unisaki Posing System al compilar.
- Corregidas compilaciones fallidas de ciertos avatares.
- Mejorado comportamiento tras cargar el avatar.
- Retirado procesamiento original de locomoción que impedía funcionar en algunos avatares.

## [2.1.1] - 2024-11-20
- Corregidos problemas de MergeAnimator en PosingOverride.

## [2.1.0] - 2024-11-20
- Resueltos pies y cadera hundidos en escritorio para permitir todas las poses correctamente.
- Cambio independiente de altura de cabeza añadido.
- Modo sin iconos para subir avatares Quest.
- Avisos ante problemas del avatar.
- Conflictos de bloqueo con AFK y bloqueo de brazos de otras herramientas resueltos.
- Bloqueo automático de cuello boca arriba añadido.

## [2.0.0] - 2024-11-12
- Versión compatible con VPM/VCC publicada.
- Nombre de herramienta en errores cambiado #1.
- Condiciones de uso añadidas #5.
