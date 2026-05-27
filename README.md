# Wing-Explosion

Juego multijugador local tipo **"papa caliente"** hecho en Unity. Un huevo-bomba pasa entre jugadores y quien lo tenga cuando explota... ¡pierde!

---

## Cómo se juega

- Capacidad de 8 jugadores.
- El huevo-bomba aparece en manos de uno de los jugadores al azar.
- **Pasa la bomba** al otro jugador antes de que explote.
- El jugador que tenga el huevo cuando el contador llegue a cero **pierde la ronda**.

### Controles

| Acción | Jugador 1 | Jugador 2 |
|--------|-----------|-----------|
| Moverse | `W A S D` | `↑ ↓ ← →` |
| Pasar el huevo | `CLICK` |

---

## 🚀 Cómo ejecutar el proyecto

### Requisitos

- [Unity Hub](https://unity.com/download)
- Unity **2021.3 LTS** o superior
- [Visual Studio](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/) con extensión C#

### Pasos

1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/Daveyify/Wing-Explosion.git
   ```

2. **Abre Unity Hub** y haz clic en **Add → Add project from disk**, luego selecciona la carpeta `Wing-Explosion`.

3. **Espera a que Unity importe los assets** (la primera vez puede tardar unos minutos).

4. En el panel **Project**, abre la escena principal ubicada en `Assets/Scenes/`.

5. Presiona el botón **▶ Play** en el editor para jugar.

---

## 📁 Estructura del proyecto

```
Wing-Explosion/
├── Assets/
│   ├── Scenes/          # Escenas del juego
│   ├── Scripts/         # Lógica del juego en C#
│   ├── Shaders/         # Shaders personalizados (ShaderLab / HLSL)
│   └── Prefabs/         # Prefabs reutilizables
├── Packages/            # Dependencias de Unity Package Manager
├── ProjectSettings/     # Configuración del proyecto Unity
└── Wing-Explosion.slnx  # Solución de Visual Studio
```

---

## 🛠️ Tech Stack

| Tecnología | Uso |
|------------|-----|
| **Unity** | Motor del juego |
| **C#** | Lógica del juego |
| **ShaderLab / HLSL** | Efectos visuales y shaders |
