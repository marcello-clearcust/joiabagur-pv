# Guía de Administración del Modelo de IA

## Descripción General

El modelo de reconocimiento de imágenes identifica productos de joyería a partir de fotografías. Este modelo se entrena en el navegador del administrador usando TensorFlow.js, sin necesidad de Python ni configuración adicional.

---

## Dashboard de Modelo de IA

### Acceso
- Navegue a **Configuración** → **Modelo de IA** o directamente a `/admin/ai-model`
- Solo accesible para usuarios con rol Administrador

### Componentes del Dashboard

#### 1. Estado del Modelo
- **Versión actual**: Identificador del modelo activo (ej: `v1737123456_20260117`)
- **Última fecha de entrenamiento**: Cuándo fue entrenado
- **Nivel de alerta**: OK, RECOMENDADO, ALTO, CRÍTICO

#### 2. Métricas del Catálogo
- Total de productos
- Productos con fotos / sin fotos
- Productos nuevos desde el último entrenamiento

#### 3. Métricas de Fotos
- Total de fotos en el sistema
- Fotos añadidas/eliminadas desde el entrenamiento
- Cambio neto (%)

#### 4. Historial de Versiones
- Tabla con versiones anteriores
- Fecha, productos usados, fotos usadas, estado

---

## Niveles de Alerta

| Nivel | Icono | Significado | Acción |
|-------|-------|-------------|--------|
| 🔴 CRÍTICO | Rojo | No hay modelo o precisión muy baja | Entrenar inmediatamente |
| 🟠 ALTO | Naranja | Muchos productos/fotos nuevos (≥10%) | Entrenar esta semana |
| 🟡 RECOMENDADO | Amarillo | Modelo desactualizado (>60 días) | Entrenar cuando sea conveniente |
| ✅ OK | Verde | Modelo actualizado | No se requiere acción |

### Notificaciones Automáticas

Cuando el nivel de alerta es CRÍTICO o ALTO:
- Se muestra un toast al iniciar sesión como administrador
- El toast incluye botón "Ver Dashboard" para acceso rápido

---

## Entrenamiento del Modelo

### Requisitos del Dispositivo

| Requisito | Mínimo | Recomendado |
|-----------|--------|-------------|
| Navegador | Chrome 90+, Edge 90+, Safari 14+, Firefox 88+ | Chrome/Edge última versión |
| WebGL | 2.0 | 2.0 con GPU dedicada |
| Memoria RAM | 2GB | 8GB+ |
| Conexión | Estable | Fibra/WiFi rápido |
| Batería | Cualquiera | Conectado a corriente |

### Duración Estimada

| Configuración | Duración |
|---------------|----------|
| Con GPU (WebGL) | 15-20 minutos |
| Sin GPU (WASM fallback) | 45-60 minutos |

La duración depende de:
- Cantidad de productos y fotos
- Potencia del hardware
- Velocidad de internet (descarga de fotos)

### Proceso de Entrenamiento

1. **Verificación de Requisitos**
   - El sistema verifica WebGL 2.0, memoria disponible
   - Muestra advertencias si está en batería

2. **Confirmación**
   - Se muestra duración estimada
   - ⚠️ **IMPORTANTE**: Mantener la pestaña abierta durante todo el proceso

3. **Descarga de Fotos** (0-30%)
   - Se descargan las fotos de productos desde el servidor
   - Típicamente 5-10MB, 2-5 minutos

4. **Carga del Modelo Base** (30-40%)
   - Se carga MobileNetV2 desde TensorFlow Hub
   - Modelo pre-entrenado para extracción de características

5. **Entrenamiento** (40-90%)
   - 15 épocas de entrenamiento
   - Muestra precisión y pérdida en tiempo real
   - La precisión típica objetivo es ≥70%

6. **Subida del Modelo** (90-100%)
   - El modelo entrenado se sube automáticamente al servidor
   - Se guarda en almacenamiento (S3/local)
   - Se actualiza la metadata

7. **Completado**
   - Mensaje de éxito con versión y precisión
   - El nuevo modelo está activo inmediatamente

### ⚠️ Advertencias Importantes

> **No cierre la pestaña durante el entrenamiento**
> El entrenamiento se ejecuta en su navegador. Cerrar la pestaña cancelará el proceso y se perderá todo el progreso.

> **Conecte su dispositivo a corriente**
> El entrenamiento es intensivo y puede agotar la batería rápidamente.

> **Cierre otras pestañas**
> Para evitar problemas de memoria, cierre otras pestañas del navegador.

---

## Solución de Problemas

### Error: "Tu dispositivo no soporta WebGL 2.0"

**Causa**: El navegador o dispositivo no tiene aceleración GPU compatible.

**Soluciones**:
1. Actualice el navegador a la última versión
2. Verifique que los drivers de GPU estén actualizados
3. Use un dispositivo diferente con mejor GPU
4. El sistema intentará usar WASM como fallback (más lento)

### Error: "No hay fotos disponibles para entrenamiento"

**Causa**: Ningún producto tiene fotos cargadas.

**Solución**:
1. Vaya a **Productos** → Seleccione un producto → **Fotos**
2. Cargue al menos 1 foto para cada producto que desee reconocer
3. Recomendación: 3-5 fotos por producto desde diferentes ángulos

### Error: "Error al cargar el modelo base MobileNetV2"

**Causa**: Problema de conexión a internet o TensorFlow Hub no disponible.

**Soluciones**:
1. Verifique su conexión a internet
2. Intente de nuevo en unos minutos
3. Verifique que no haya bloqueos de firewall a `tfhub.dev`

### El entrenamiento se congela o es muy lento

**Posibles causas**:
- Poca memoria RAM disponible
- Muchas pestañas abiertas
- Dispositivo sin GPU

**Soluciones**:
1. Cierre otras aplicaciones y pestañas
2. Reinicie el navegador antes de entrenar
3. Use un dispositivo con más recursos
4. Espere pacientemente (CPU puede tomar 45-60 min)

### El entrenamiento falla a mitad del proceso

**Causa**: Error de memoria (OOM) o desconexión.

**Soluciones**:
1. El modelo anterior sigue activo (no se pierde)
2. Cierre otras pestañas para liberar memoria
3. Intente de nuevo
4. Si persiste, use un dispositivo con más RAM

### La precisión del modelo es baja (<60%)

**Posibles causas**:
- Pocas fotos de entrenamiento
- Fotos de mala calidad
- Productos muy similares visualmente

**Soluciones**:
1. Añada más fotos por producto (3-5 mínimo)
2. Use fotos con buena iluminación y fondo limpio
3. Capture productos desde diferentes ángulos
4. Reentrene después de añadir más fotos

---

## Cuándo Reentrenar

### Escenarios que Requieren Reentrenamiento

| Escenario | Urgencia |
|-----------|----------|
| Nuevos productos añadidos | ALTO si >10% del catálogo |
| Fotos actualizadas/añadidas | RECOMENDADO si >20% de fotos |
| Precisión baja reportada por operadores | ALTO |
| Modelo tiene >60 días | RECOMENDADO |
| Primer despliegue (sin modelo) | CRÍTICO |

### Mejores Prácticas

1. **Entrene fuera de horas pico**: El entrenamiento no afecta el servidor, pero su dispositivo estará ocupado
2. **Mantenga el modelo actualizado**: Reentrene al menos 1-2 veces al mes si hay cambios en el catálogo
3. **Verifique la precisión**: Después de entrenar, pruebe con algunos productos para confirmar
4. **Documente versiones**: El historial muestra todas las versiones anteriores

---

## Arquitectura Técnica

### Modelo Base
- **MobileNetV2**: Red neuronal pre-entrenada en ImageNet
- Optimizada para dispositivos móviles
- Tamaño del modelo: ~14MB

### Entrenamiento (Transfer Learning)
- Se usa MobileNetV2 como extractor de características
- Se entrena una capa de clasificación personalizada
- 15 épocas, batch size 32, validation split 20%

### Formato del Modelo
- TensorFlow.js (model.json + weights.bin)
- Compatible con navegadores modernos
- Caché automático después de primera descarga

### Almacenamiento
- Modelos guardados en S3 (producción) o filesystem local (desarrollo)
- Versionado por timestamp
- Modelo anterior preservado si falla el nuevo
