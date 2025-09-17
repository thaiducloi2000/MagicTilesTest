# 🎮 Rhythm Tile Clicker (Unity Test)

## 📌 Giới thiệu
**Tile-based rhythm gameplay**:
---

## ▶️ Hướng dẫn chạy
1. Clone repo và mở folder với **Unity 2022.3.xxxx**.
2. Scene chính: **`Gameplay.unity`**.
3. Nhấn **Play** → tile sẽ rơi xuống, người chơi click/hold để chơi.
4. Điểm và feedback hiển thị trên UI.

---

## 🏗️ Kiến trúc & Class chính

### 🔹 Tile System
- **Tile (base class)**  
  - Quản lý trạng thái click, drop down, callbacks/event.  
- **TapTile**  
  - Người chơi click một lần để nhận điểm.  
- **HoldTile**  
  - Người chơi giữ trong khoảng thời gian `_timeHold`.  
  - Kết quả Perfect/Good dựa vào thời gian giữ.

### 🔹 Spawner
- **TileSpawner**  
  - Spawn tile theo interval.  
  - Sử dụng **ObjectPool\<Tile\>** (dictionary) để tái sử dụng tile.  
  - Lắng nghe event **TileMiss** để trả tile về pool.

### 🔹 Score Manager
- **ScoreManager**  
  - Lắng nghe sự kiện **TileClickEvent** / **TileMissEvent**.  
  - Quản lý điểm số và combo.  
  - Publish event `OnPointChangeData` để UI cập nhật.

### 🔹 UI
- **UIPoint**  
  - Hiển thị điểm và timing (Perfect/Good/Miss).  
  - Có hiệu ứng **Tween Animation** khi điểm thay đổi.
- **UITextScaleUpAnimation**  
  - Animation scale text lên rồi về lại để nhấn mạnh thay đổi.

### 🔹 Event Bus
- Repo: [Event Bus](https://github.com/thaiducloi2000/event_bus)  
- Dùng để **decouple hệ thống**: Tile → EventBus → ScoreManager → UI.

### 🔹 Tween Animation
- **TweenDoAnimation**: base cho animation one-shot.  
- **TweenLoopAnimation**: base cho animation loop.  
- **TweenClickScale / UITextScaleUpAnimation**: scale effect khi click hoặc khi score tăng.

---

## ⚙️ Flow Gameplay
1. **Spawner** tạo tile (Tap/Hold).  
2. Tile rơi xuống (DropDown).  
3. Người chơi click:  
   - TapTile → Perfect/Good ngay.  
   - HoldTile → giữ, sau khi thả/đủ thời gian mới Judge Perfect/Good.  
4. Tile publish event → **ScoreManager** nhận → update điểm/combo.  
5. **UIPoint** nhận event → update UI + play animation.  
