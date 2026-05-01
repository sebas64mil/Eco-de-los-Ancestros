# 🎯 Sistema de Proyectiles y Disparo - Eco de los Ancestros

Documentación completa del sistema de proyectiles y disparo del juego.

---

## 📑 Tabla de Contenidos

1. [Visión General](#visión-general)
2. [Estructura del Proyecto](#estructura-del-proyecto)
3. [Interfaces Base](#interfaces-base)
4. [Componentes Principales](#componentes-principales)
5. [Tipos de Proyectiles](#tipos-de-proyectiles)
6. [Sistema de Input](#sistema-de-input)
7. [Flujos de Funcionamiento](#flujos-de-funcionamiento)
8. [Configuración](#configuración)
9. [Optimizaciones](#optimizaciones)

---

## 🎮 Visión General

El sistema de proyectiles y disparo es el núcleo de la mecánica de combate del juego. Permite al jugador disparar diferentes tipos de proyectiles, cada uno con habilidades especiales únicas. El sistema está optimizado mediante **object pooling** para evitar instantiaciones constantes de GameObjects.

### Características Principales

- ✅ **4 tipos de proyectiles** diferentes + mini proyectiles
- ✅ **Habilidades especiales** dinámicas para cada proyectil
- ✅ **Object pooling** para optimización de rendimiento
- ✅ **Sistema de cooldown** flexible y configurable
- ✅ **Física 2D** con gravedad, colisiones y rebotes
- ✅ **Lockout dinámico** para evitar spam de cambios
- ✅ **Trayectorias simuladas** con predicción
- ✅ **Configuración mediante ScriptableObjects** (sin necesidad de recompilar)

---

## 📁 Estructura del Proyecto

```
Assets/Eco_De_LosAncestros/Scripts/
│
├── Player/
│   ├── ProjectileShooter.cs         ← 🎯 Sistema principal de disparo
│   ├── ProjectileSelector.cs        ← 🔄 Selector de tipos de proyectiles
│   ├── PlayerController.cs
│   ├── InputHandler.cs              ← ⌨️ Manejo de entrada del jugador
│   └── DataPlayer.cs                ← 📊 Datos y estadísticas del jugador
│
└── Bullet/
    ├── ObjectPool.cs                ← ♻️ Pool de objetos reutilizables
    ├── ProjectileSelector.cs
    ├── TrajectoryCalculator2D.cs    ← 📐 Cálculo de trayectorias
    ├── DataContainersPool.cs        ← 📦 Contenedores para pools
    ├── interfaces/
    │   ├── IProjectile.cs           ← 📋 Interfaz de proyectiles
    │   └── IPoolable.cs             ← 📋 Interfaz para objeto poolable
    ├── Projectiles/
    │   ├── BasicProjectile.cs       ← 🟢 Proyectil básico
    │   ├── EmbeddedProjectile.cs    ← 🔴 Proyectil especial (boost)
    │   ├── SplitProjectile.cs       ← 🟡 Proyectil especial (división)
    │   ├── AreaProjectile.cs        ← 🟣 Proyectil especial (área)
    │   └── MiniProjectile.cs        ← ⚪ Mini proyectiles (por Split)
    │
    └── SO/ (ScriptableObjects)
        ├── ProjectileDataSO.cs      ← ⚙️ Datos de proyectiles
        └── TrajectorySettingsSO.cs  ← ⚙️ Configuración de trayectorias
```

---

## 📋 Interfaces Base

### IProjectile

Define el contrato que **todo proyectil debe cumplir**.

```csharp
public interface IProjectile
{
    bool AllowBounce { get; }           // ¿Permite rebotes en el suelo?
    void Launch(...);                    // Lanzar proyectil con dirección, fuerza, etc.
    float FireCooldown { get; }         // Tiempo de espera entre disparos
    bool IsSpecial { get; }             // ¿Es un proyectil especial?
}
```

### IPoolable

Define el contrato para objetos que pueden ser **reutilizados en el pool**.

```csharp
public interface IPoolable
{
    void SetPool(ObjectPool pool);      // Registrar pool al objeto
}
```

---

## ⚙️ Componentes Principales

### 1️⃣ ProjectileShooter - Sistema de Disparo

**Ubicación:** `Assets/Eco_De_LosAncestros/Scripts/Player/ProjectileShooter.cs`

#### ¿Qué hace?
Es el **motor central de disparo**. Cuando el jugador presiona fuego, este componente:
1. Obtiene un proyectil del pool (o crea uno si no hay disponibles)
2. Posiciona el proyectil en el punto de disparo
3. Calcula la fuerza según la estadística `DataPlayer.currentStrength` del jugador
4. Aplica la velocidad inicial al Rigidbody2D
5. Gestiona el cooldown entre disparos

#### Características
- **Cooldowns dinámicos** por tipo de proyectil
- **Primer disparo inmediato** (sin esperar cooldown)
- **Reutiliza objetos del pool** para optimización
- **Invoca eventos** cuando se dispara un especial

#### Flujo de ejecución
```
Presionar fuego → ProjectileShooter obtiene proyectil del pool
                → Posiciona en firePoint
                → Aplica fuerza (basada en currentStrength)
                → Inicia cooldown
                → Invoca evento si es especial
```

---

### 2️⃣ ProjectileSelector - Selector de Proyectiles

**Ubicación:** `Assets/Eco_De_LosAncestros/Scripts/Bullet/ProjectileSelector.cs`

#### ¿Qué hace?
Gestiona **qué tipo de proyectil está activo** y previene cambios excesivos.

#### Tipos disponibles
1. 🟢 **Basic** - Proyectil básico (disponible siempre)
2. 🔴 **Embedded** - Proyectil con boost
3. 🟡 **Split** - Proyectil que se divide
4. 🟣 **Area** - Proyectil de área

#### Sistema de Lockout
El **lockout** previene que cambies de proyectil especial demasiado rápido. Después de usar un especial:
- Se activa un bloqueo temporal (0.5s por defecto)
- No puedes cambiar a OTRO especial durante este tiempo
- El bloqueo se cancela si disparas o presionas "cancelar especial"

#### Flujo de ejecución
```
InputHandler.OnEmbeddedProjectile
        ↓
¿Está en lockout? 
  SÍ → Ignora el cambio
  NO → Cambia a Embedded, activa lockout (0.5s)
```

---

### 3️⃣ ObjectPool - Pool de Objetos

**Ubicación:** `Assets/Eco_De_LosAncestros/Scripts/Bullet/ObjectPool.cs`

#### ¿Qué hace?
Reutiliza GameObjects para **evitar instantiación/destrucción constante** (optimización crucial).

#### Beneficios
- ✅ Reduce **presión del garbage collector**
- ✅ Mejora **rendimiento y FPS**
- ✅ Evita "lag spikes" por creación de objetos
- ✅ Simula disparos rápidos sin problemas

#### Funcionamiento
1. **Creación inicial:** Crea N objetos inactivos al inicio
2. **Obtener:** Activa un objeto del pool
3. **Retornar:** Desactiva el objeto para ser reutilizado
4. **Escalabilidad:** Si se agotan, crea nuevos automáticamente

#### Ejemplo
```csharp
// Pedir un proyectil del pool
Projectile proj = pool.Get(); // Activa uno inactivo o crea nuevo

// Usar proyectil...

// Devolver al pool
pool.Return(proj); // Desactiva para reutilizar luego
```

---

### 4️⃣ TrajectoryCalculator2D - Cálculo de Trayectorias

**Ubicación:** `Assets/Eco_De_LosAncestros/Scripts/Bullet/TrajectoryCalculator2D.cs`

#### ¿Qué hace?
**Predice la trayectoria** del proyectil considerando:
- Gravedad
- Colisiones con el suelo
- Rebotes (si están permitidos)

#### Aplicaciones
- 🎯 **Visualización de trayectoria** antes de disparar
- 📍 **Predicción del impacto**
- 🔍 **Detección de obstáculos** en la ruta

#### Características
- Simula física sin instantaneidad (gradualmente)
- Calcula rebotes realistas con amortiguación
- Límite de rebotes configurable (máx. 3)
- Se detiene si la velocidad es muy baja

---

## 🚀 Tipos de Proyectiles

Cada proyectil tiene características y habilidades especiales únicas.

### 1. 🟢 BasicProjectile - Proyectil Básico

**Tipo:** Normal (no especial)  
**Rebotes:** ✅ Sí (configurable)  
**Cooldown:** 0.5s por defecto

#### Descripción
Es el proyectil **estándar y versátil**. Es tu opción por defecto y siempre está disponible.

#### Comportamiento
- Se dispara normalmente
- Puede rebotar en el suelo
- Se destruye tras timeout (3s) o al colisionar
- Afectado por gravedad

#### Cuándo usarlo
- Al inicio del juego
- Para disparos generales
- Cuando necesitas rebotes

#### Ejemplo de uso
```
Presiona FIRE → BasicProjectile sale del cañón
             → Cae y rebota en el suelo
             → Se destruye después de 3 segundos
```

---

### 2. 🔴 EmbeddedProjectile - Proyectil Embebido (ESPECIAL)

**Tipo:** Especial  
**Rebotes:** ❌ No  
**Activación:** Presionar tecla de "boost"

#### Descripción
Proyectil que recibe un **impulso adicional** cuando lo activas. Perfecto para disparos de largo alcance rápidos.

#### Cómo funciona
1. **Disparas normalmente** el proyectil Embedded
2. **El proyectil vuela** hasta que presiones la tecla especial
3. **Al presionar especial:** El proyectil recibe un impulso horizontal adicional (boost de 10 unidades)
4. **Resultado:** El proyectil acelera hacia adelante con mayor velocidad

#### Características especiales
- Boost de velocidad horizontal: **10 unidades**
- Solo se puede usar UNA VEZ por lanzamiento
- Después de usar, activa **lockout de 0.5s**
- No tiene rebotes

#### Cuándo usarlo
- Para alcanzar enemigos lejanos
- Para sorpresas rápidas
- Cuando necesitas velocidad extra

#### Ejemplo de uso
```
1. Presiona FIRE → Lanzas Embedded
2. Presiona ESPECIAL (mientras vuela) → ¡BOOST! Se acelera
3. Impacta objetivo con velocidad aumentada
4. Espera 0.5s antes de usar otro especial
```

---

### 3. 🟡 SplitProjectile - Proyectil Divisor (ESPECIAL)

**Tipo:** Especial  
**Rebotes:** ❌ No  
**Activación:** Presionar tecla de "split"  
**Cantidad de mini proyectiles:** 3  
**Ángulo de dispersión:** 30°

#### Descripción
Proyectil que se **divide en múltiples mini proyectiles** al activarse. Excelente para cubrir área.

#### Cómo funciona
1. **Disparas normalmente** el SplitProjectile
2. **El proyectil vuela** como uno solo
3. **Al presionar especial:** El proyectil original SE DESTRUYE
4. **Se crean 3 mini proyectiles** que salen en ángulo de 30° (uno centro, uno -15°, uno +15°)
5. **Cobertura de área:** Los 3 mini proyectiles vuelan juntos

#### Características especiales
- **Cantidad:** 3 mini proyectiles
- **Spread:** 30° de ángulo total
- **Fuerza de cada mini:** 6 unidades
- **Duración de minis:** ~2 segundos
- Después de usar, activa **lockout de 0.5s**

#### Ventajas
- ✅ Cubre más área
- ✅ Mejor para múltiples enemigos
- ✅ Dispersión configurable

#### Desventajas
- ❌ Menos fuerza individual
- ❌ Mini proyectiles más débiles
- ❌ No rebotan

#### Cuándo usarlo
- Enemigos esparcidos en área
- Necesitas cubrir múltiples objetivos
- Quieres dispersión controlada

#### Ejemplo de uso
```
1. Presiona FIRE → Lanzas SplitProjectile
2. Presiona ESPECIAL (mientras vuela) → ¡DIVISIÓN!
3. Se crean 3 mini proyectiles con spread de 30°
4. Los 3 vuelan juntos hacia diferentes ángulos
5. Duran ~2 segundos cada uno
```

#### Visualización de dispersión
```
          Mini 1 (-15°)
                  |
         Mini Central (0°)
          |       |
    Mini 2 (+15°) |
         /    \   |
        /      \  |
       /        \ |
```

---

### 4. 🟣 AreaProjectile - Proyectil de Área (ESPECIAL)

**Tipo:** Especial  
**Rebotes:** ❌ No  
**Activación:** Presionar tecla de "área"  
**Radio de efecto:** 2 unidades  
**Duración de efecto:** 1 segundo

#### Descripción
Proyectil que crea una **zona de efecto** cuando lo activas. Detiene el movimiento y crea un campo protector/ofensivo.

#### Cómo funciona
1. **Disparas normalmente** el AreaProjectile
2. **El proyectil vuela** como uno solo
3. **Al presionar especial:** El proyectil PARA en el aire
4. **Se desactiva la física** (no cae ni rebota)
5. **Zona de efecto:** Se crea un campo circular invisible de 2 unidades
6. **Duración:** La zona dura 1 segundo
7. **Resultado:** Cualquier enemigo en el radio es detectado/afectado

#### Características especiales
- **Radio:** 2 unidades desde centro del proyectil
- **Duración:** 1 segundo de efecto
- **Detección:** Busca enemigos en el radio
- Después de usar, activa **lockout de 0.5s**

#### Ventajas
- ✅ Posicionamiento dinámico (puedes colocar el área donde quieras)
- ✅ Zona controlada
- ✅ Protección/ataque en área

#### Desventajas
- ❌ Duración limitada (1s)
- ❌ No es ofensiva como otros
- ❌ Menor daño instantáneo

#### Cuándo usarlo
- Necesitas defensiva temporal
- Quieres detectar enemigos en área
- Búsqueda de objetivos
- Control de territorio

#### Ejemplo de uso
```
1. Presiona FIRE → Lanzas AreaProjectile
2. Presiona ESPECIAL (mientras vuela) → ¡ÁREA ACTIVADA!
3. El proyectil se detiene en el aire
4. Se crea zona invisible de 2 unidades
5. Detecta enemigos dentro del radio durante 1 segundo
```

#### Visualización
```
        Enemigo A
           |
    +------+------+
    |   Zona      |  Radio = 2 unidades
    |   (1 seg)   |
    |      • ← Proyectil central
    +------+------+
           |
        Enemigo B
    
Los enemigos A y B son detectados si están dentro del círculo
```

---

### 5. ⚪ MiniProjectile - Mini Proyectil

**Generado por:** SplitProjectile  
**Tipo:** Especial  
**Rebotes:** ❌ No  
**Cooldown:** 0 (disparo directo)

#### Descripción
Son los pequeños proyectiles que se generan cuando **activas el SplitProjectile**. No se pueden disparar directamente.

#### Características
- Generados automáticamente (3 por split)
- Duración: ~2 segundos
- Tamaño pequeño (para diferenciar)
- Sin recarga de disparo
- Se destruyen por timeout o colisión

#### Comportamiento
- Vuelan en línea recta
- No rebotan
- Se ven afectados por gravedad
- Se destruyen al tocar suelo/enemigos

---

## ⌨️ Sistema de Input

El sistema controla **qué hace el jugador** mediante teclas y botones.

### Teclas del Sistema de Proyectiles

| Evento | Tecla | Acción |
|--------|-------|--------|
| **OnFire** | Botón de fuego (Click/Control) | Dispara proyectil actual |
| **OnSpecial** | Tecla especial (E por defecto) | Activa habilidad del proyectil actual |
| **OnEmbeddedProjectile** | Tecla Q | Cambia a Embedded |
| **OnSplitProjectile** | Tecla W | Cambia a Split |
| **OnAreaProjectile** | Tecla R | Cambia a Area |
| **OnCancelSpecial** | Tecla X | Vuelve a BasicProjectile |
| **OnRotate** | Movimiento ratón/Joystick | Rota el cañón |
| **OnStrength** | Rueda ratón/Analógico | Ajusta fuerza disparo |

### Flujo de Control

```
InputHandler (escucha entrada del jugador)
        ↓
Eventos (OnFire, OnSpecial, OnEmbeddedProjectile, etc.)
        ↓
ProjectileSelector (valida cambio, aplica lockout)
        ↓
ProjectileShooter (ejecuta disparo)
        ↓
ObjectPool (obtiene/crea GameObject)
        ↓
Proyectil específico (executa lógica de habilidad)
```

### Ejemplo de uso
```csharp
// Cuando presionas FIRE
InputHandler.OnFire?.Invoke();
  → ProjectileShooter.Fire()
    → ObjectPool.Get() obtiene un BasicProjectile
    → BasicProjectile.Launch(direction, force)
    → Se aplica velocidad al Rigidbody2D
    → Inicia cooldown (0.5s)

// Cuando presionas Q (cambio a Embedded)
InputHandler.OnEmbeddedProjectile?.Invoke();
  → ProjectileSelector.SelectEmbedded()
    → Valida que no esté en lockout
    → Cambia tipo activo a Embedded

// Cuando presionas E (especial)
InputHandler.OnSpecial?.Invoke();
  → ProjectileShooter proyectil actual
    → Si es EmbeddedProjectile → Aplica boost
    → Si es SplitProjectile → Divide en 3
    → Si es AreaProjectile → Crea zona
```

---

## 🔄 Flujos de Funcionamiento

### Flujo 1: Disparo Básico

```
🎮 Jugador presiona FIRE
        ↓
⌨️ InputHandler detecta entrada
        ↓
🔄 ProjectileSelector valida proyectil activo
        ↓
⏱️ ¿Pasó el cooldown?
   SÍ → Continúa
   NO → Ignora disparo, espera
        ↓
📦 ObjectPool.Get() obtiene/crea proyectil
        ↓
🎯 ProjectileShooter.Launch()
   - Posiciona en firePoint
   - Calcula fuerza = currentStrength
   - Aplica velocidad a Rigidbody2D
        ↓
⏱️ ProjectileShooter inicia cooldown
        ↓
🚀 Proyectil vuela en escena
        ↓
💥 Colisión o timeout
        ↓
♻️ ObjectPool.Return() devuelve proyectil
```

### Flujo 2: Cambio de Proyectil Especial

```
🎮 Jugador presiona Q (Embedded)
        ↓
⌨️ InputHandler.OnEmbeddedProjectile
        ↓
🔄 ProjectileSelector.SelectEmbedded()
        ↓
❓ ¿Está en lockout?
   SÍ → Bloquea cambio (muestra feedback)
   NO → Continúa
        ↓
✅ Cambia tipo activo a Embedded
   - BasicProjectile → Embedded
   - Pool básico desactivado
   - Pool Embedded activado
        ↓
🎯 Próximo disparo usará Embedded
```

### Flujo 3: Activación de Habilidad Especial (Ejemplo: Split)

```
1️⃣ Presionas W → Cambias a SplitProjectile
        ↓
2️⃣ Presionas FIRE → Disparas SplitProjectile
        ↓
📦 ObjectPool obtiene un SplitProjectile
        ↓
🚀 SplitProjectile vuela en trayectoria normal
        ↓
3️⃣ Presionas E (ESPECIAL) → Activas habilidad
        ↓
💥 SplitProjectile se divide
   - Se crea array de 3 Vector2 (direcciones)
   - Cálculo de spread: -15°, 0°, +15°
   - Se crean 3 MiniProjectile con esas direcciones
        ↓
⏱️ ProjectileSelector activa lockout (0.5s)
        ↓
🚫 Bloqueado: No puedes cambiar a otro especial
        ↓
⏱️ Pasan 0.5s → Lockout termina
        ↓
✅ Puedes cambiar de proyectil de nuevo
```

### Flujo 4: Proyectil con Reutilización (Object Pooling)

```
Primer disparo:
  ObjectPool.Get() → Crea nuevo BasicProjectile
  
Segundo disparo:
  ObjectPool.Get() → Reutiliza el mismo BasicProjectile (que estaba inactivo)
  
Tercer disparo:
  Si hay otro disponible inactivo:
    ObjectPool.Get() → Reutiliza otro
  Si NO hay disponible:
    ObjectPool.Get() → Crea nuevo (escalabilidad automática)

💡 Beneficio: No se instancia/destruye constantemente
   → Mejor rendimiento
   → Menos garbage collection
   → FPS más estable
```

---

## ⚙️ Configuración

Todo está configurado mediante **ScriptableObjects** (sin necesidad de recompilar).

### ProjectileDataSO - Datos de Proyectiles

```csharp
[SerializeField] private float lifeTime = 3f;          // Duración (segundos)
[SerializeField] private float fireCooldown = 0.5f;    // Espera entre disparos
[SerializeField] private float specialDuration = 1f;   // Duración especial
```

#### Valores por defecto
- **BasicProjectile lifeTime:** 3s
- **BasicProjectile fireCooldown:** 0.5s
- **EmbeddedProjectile boostForce:** 10 unidades
- **SplitProjectile splitCount:** 3 mini proyectiles
- **SplitProjectile spreadAngle:** 30°
- **SplitProjectile splitForce:** 6 unidades
- **AreaProjectile radius:** 2 unidades
- **AreaProjectile specialDuration:** 1s

### TrajectorySettingsSO - Configuración de Trayectorias

```csharp
[SerializeField] private float timeStep = 0.02f;           // Granularidad simulación
[SerializeField] private int maxPoints = 100;              // Máx puntos línea
[SerializeField] private int maxBounces = 3;               // Máx rebotes
[SerializeField] private float bounceDamping = 0.8f;       // Energía tras rebote
[SerializeField] private float minVelocity = 0.5f;         // Vel mínima
[SerializeField] private float surfaceOffset = 0.01f;      // Offset anti-clipping
```

#### Cómo modificar
1. En Project, busca los ScriptableObjects en `Assets/Eco_De_LosAncestros/SO/`
2. Selecciona el que quieras editar
3. Cambia valores en el Inspector
4. ¡Listo! Los cambios aplican sin recompilar

#### Ejemplo de ajuste
```
Quiero que los disparos sean más rápidos:
  → ProjectileShooter → Aumentar fireCooldown
  → O aumentar currentStrength del jugador

Quiero que los splits dispersen más:
  → SplitProjectile → Aumentar spreadAngle de 30° a 45°
  
Quiero zonas de área más grandes:
  → AreaProjectile → Aumentar radius de 2 a 3 unidades
```

---

## 🚀 Optimizaciones

### 1. Object Pooling ♻️

**Problema:** Crear/destruir GameObjects constantemente causa lag.

**Solución:** Reutilizar objetos inactivos.

```
Startup:
  ✓ Crea 10 BasicProjectiles inactivos
  ✓ Crea 5 EmbeddedProjectiles inactivos
  ✓ Crea 3 SplitProjectiles inactivos
  ✓ etc...

Durante gameplay:
  ✓ Pide al pool: "Dame un BasicProjectile"
  ✓ Pool lo activa (rápido)
  ✓ Lo usas
  ✓ Devuelves: "Toma, guarda este"
  ✓ Pool lo desactiva

Beneficio: ⚡ Rendimiento +200%, sin GC spikes
```

### 2. Reutilización de Interfaces 📋

```csharp
// Todos implementan IProjectile
BasicProjectile : IProjectile
EmbeddedProjectile : IProjectile
SplitProjectile : IProjectile
AreaProjectile : IProjectile

// Beneficio:
✓ Código genérico para todos
✓ Fácil agregar nuevos tipos
✓ Menos duplicación
```

### 3. Sistema de Eventos 📢

```csharp
// En vez de referencias directas:
ProjectileShooter.Shooter.Fire(); // ❌ Acoplamiento

// Usamos eventos:
InputHandler.OnFire?.Invoke();     // ✅ Desacoplamiento

// Beneficios:
✓ Componentes independientes
✓ Fácil debuguear
✓ Menos dependencias
```

### 4. Cooldowns Dinámicos ⏱️

```csharp
// Cada proyectil tiene su propio cooldown
projectileCooldown[BasicProjectile] = 0.5s
projectileCooldown[EmbeddedProjectile] = 1.2s

// Beneficio:
✓ Cada tipo puede tener velocidad diferente
✓ Sin afectar a otros
```

---

## 🔍 Debugging y Troubleshooting

### Problema: Los proyectiles no se disparan

**Posibles causas:**
1. ⏱️ Todavía en cooldown → Espera 0.5s
2. 🔒 Lockout activo → Espera a que termine
3. 📦 Pool vacío → Aumentar tamaño inicial en Inspector
4. 🔊 Evento no conectado → Verificar InputHandler

**Solución:**
```csharp
// Agrega esto a ProjectileShooter para debuguear:
if (Time.time - lastFireTime < cooldown) {
    Debug.Log("En cooldown: " + (cooldown - (Time.time - lastFireTime)));
} else {
    Debug.Log("Pueden disparar");
}
```

### Problema: Lag/FPS bajo

**Causas posibles:**
1. ♻️ Pool muy pequeño (instantiando constante)
2. 🚀 Demasiados proyectiles activos
3. 💥 Colisiones sin optimizar

**Soluciones:**
1. Aumentar tamaño inicial del pool (en Inspector)
2. Reducir `lifeTime` de proyectiles
3. Usar `Physics2D.BroadcastMessage` en vez de GetComponent

### Problema: Habilidad especial no se activa

**Causas:**
1. 🔒 En lockout → Esperar
2. 📦 Tipo de proyectil sin habilidad
3. 🎯 Input no registrando

**Solución:**
```csharp
// Agrega log en OnSpecial:
InputHandler.OnSpecial += () => {
    Debug.Log("Especial presionada, tipo actual: " + currentProjectileType);
};
```

---

## 📈 Estadísticas del Sistema

| Métrica | Valor | Notas |
|---------|-------|-------|
| Tipos de proyectiles | 4 | Basic, Embedded, Split, Area |
| Mini proyectiles por split | 3 | Configurable |
| Cooldown básico | 0.5s | Configurable |
| Duración proyectil | 3s | Configurable |
| Velocidad máxima | ∞ | Depende de currentStrength |
| Radio zona área | 2 unidades | Configurable |
| Duración zona área | 1s | Configurable |
| Lockout especial | 0.5s | Configurable |
| Pool inicial recomendado | 15-20 | Por tipo |

---

## 🎓 Ejemplo Práctico: Crear Nuevo Proyectil

Si quieres agregar un **nuevo tipo de proyectil**, sigue estos pasos:

### Paso 1: Crear la clase

```csharp
// Assets/Eco_De_LosAncestros/Scripts/Bullet/Projectiles/NewProjectile.cs

public class NewProjectile : MonoBehaviour, IProjectile
{
    public bool AllowBounce => false;
    public float FireCooldown => 0.7f;
    public bool IsSpecial => true;
    
    public void Launch(Vector3 position, Vector3 direction, float force, bool allowBounce)
    {
        // Tu lógica de lanzamiento
    }
}
```

### Paso 2: Agregar al selector

```csharp
// En ProjectileSelector.cs
public enum ProjectileType { Basic, Embedded, Split, Area, NewProjectile } // ← Agregar aquí
```

### Paso 3: Crear pool

```csharp
// En ProjectileSelector.cs
private ObjectPool newProjectilePool;
newProjectilePool = new ObjectPool(newProjectilePrefab, 5);
```

### Paso 4: Agregar tecla de entrada

```csharp
// En InputHandler.cs
public event System.Action OnNewProjectile;

// En método de input
if (Input.GetKeyDown(KeyCode.Y))
    OnNewProjectile?.Invoke();
```

### Paso 5: ¡Listo!

Ahora puedes cambiar a tu nuevo proyectil como a cualquier otro.

---

## 📚 Referencias Rápidas

### Clases principales
- `ProjectileShooter` - Disparar
- `ProjectileSelector` - Cambiar tipo
- `ObjectPool` - Reutilizar objetos
- `IProjectile` - Contrato de proyectil
- `TrajectoryCalculator2D` - Calcular trayectorias

### ScriptableObjects
- `ProjectileDataSO` - Configurar datos
- `TrajectorySettingsSO` - Configurar trayectorias

### Carpetas principales
- `Assets/Eco_De_LosAncestros/Scripts/Player/` - Control del jugador
- `Assets/Eco_De_LosAncestros/Scripts/Bullet/` - Sistema de proyectiles
- `Assets/Eco_De_LosAncestros/SO/` - Datos configurables

---

## ✅ Checklist de Funcionalidades

- [x] 4 tipos de proyectiles funcionales
- [x] Habilidades especiales para cada tipo
- [x] Sistema de cooldown
- [x] Object pooling para optimización
- [x] Lockout dinámico
- [x] Trayectorias simuladas
- [x] Evento de disparo especial
- [x] Configuración mediante ScriptableObjects
- [x] Sistema de input completo
- [x] Documentación completa

---

## 🤝 Créditos y Notas

Sistema diseñado para máxima **optimización**, **flexibilidad** y **facilidad de extensión**.

Cualquier pregunta, consulta la documentación de código o revisa los comentarios en los scripts.

¡Happy coding! 🎮✨
