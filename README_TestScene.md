# Тестовая сцена Astra Bricks

`TestProject` — песочница прототипа: поле, бита, мяч и параллакс-фон. Блоков, сетки и HUD
в проекте пока нет. Играется в `Assets/Scenes/TestScene.unity`.

- Unity **6000.5.5f1**, URP, Input System (новый), только портрет.
- Дизайн — `..\Docs\Design\GDD_AstraBricks.md`, технические требования — `..\Docs\Tech\TR_AstraBricks.md`.
- В проекте нет системы контроля версий: перед рискованными правками делай копию `Assets` и `ProjectSettings`.

## Состав сцены

```
Main Camera        PlayfieldCamera (вписывает поле), GameConfig
Background         BG_Far_Nebula   — дальний слой, order -20, дрейф 0.35 / 0.10
                   BG_Near_Planets — ближний слой, order -10, дрейф 1.10 / 0.30
Game               FieldBounds (стены), дети:
  ├─ Paddle        PaddleController + BoxCollider2D, ребёнок Sprite
  ├─ Ball          BallController + SpriteRenderer
  ├─ Wall_Left     BoxCollider2D, без рендерера
  ├─ Wall_Right    BoxCollider2D, без рендерера
  └─ Wall_Top      BoxCollider2D, без рендерера
```

## Мир и камера

`1 единица = 1% ширины поля`, поле **100 × 170**, центр — (0, 0). `PlayfieldCamera` считает
ортографический размер как `max(170/2, (100/2) / aspect)`: поле всегда видно целиком по меньшей
стороне экрана, остальное отдаётся фону, масштаб по осям единый.

| Экран | Кадр в единицах | Что видно |
|---|---|---|
| 9:16 | 100 × 177.8 | поле по ширине, узкие полосы фона сверху и снизу |
| 9:19.5 | 100 × 216.7 | поле по ширине, больше фона по вертикали |
| 4:3 | 226.7 × 170 | поле по высоте, широкие поля фона по бокам |

## GameConfig

`Assets/Config/GameConfig.asset` — единственное место, где живут числа. Значения читаются каждый
кадр, поэтому в Play их можно менять на ходу и сразу видеть результат.

| Параметр | Значение | Смысл |
|---|---|---|
| `fieldWidth` / `fieldHeight` | 100 / 170 | размер поля в единицах |
| `wallThickness` | 2 | толщина стен-коллайдеров |
| `paddleWidth` / `paddleHeight` | 25 / 5 | размер биты (1/4 ширины поля) |
| `paddleHeightAboveBottom` | 12 | высота центра биты над низом поля |
| `swipeSensitivity` | 50 | множитель свайпа (см. «Бита») |
| `keyboardSpeed` | 120 | скорость стрелками, единиц/с |
| `paddleFollowTime` | 0.2 | постоянная сглаживания биты, с (0 = без сглаживания) |
| `boundDirection` | 65 | максимальный угол отскока от биты, градусы |
| `ballSpeed` | 80 | скорость мяча, единиц/с |
| `ballRadius` | 2 | радиус мяча |
| `maxDeviation` | 0.9 | защита от почти горизонтального полёта |
| `randomDeviation` | 0.1 | случайное отклонение после отскока |
| `autoStartDelay` | 3 | авто-старт мяча, с |
| `startAngleSpread` | 5 | разброс угла при старте, градусы |
| `sideRayCooldownFrames` | 5 | пауза между боковыми лучами, кадры |

## Бита

`PaddleController`. Размеры и поведение — из конфига, каждый кадр вызывается `ApplySize()`:
коллайдер и дочерний спрайт подгоняются под `paddleWidth` / `paddleHeight`, так что размер можно
менять в Play.

- **Свайп относительный**: бита сдвигается на `Δпикселей × WorldPerPixel() × swipeSensitivity`,
  а не встаёт под палец. `WorldPerPixel()` — ширина кадра в единицах, делённая на ширину экрана в
  пикселях. При поле 1080 px и `swipeSensitivity = 50` это ≈ 4.6 единицы на пиксель пальца, то есть
  полная ширина поля — за ~22 px движения. Число подобрано на тесте; на новом разрешении проверять
  заново.
- **Сглаживание**: `x → lerp(x, targetX, 1 − exp(−dt / paddleFollowTime))`; при 0 бита встаёт сразу.
- **Кламп**: `±(fieldWidth/2 − половина ширины биты)` = ±37.5 при текущих числах.
- **Клавиатура**: стрелки и A/D, скорость `keyboardSpeed`.
- **Отскок**: `GetBoundDirection(point)` — угол от −65° до +65° по точке удара, край биты даёт
  максимальный угол.
- `TopEdgeY` — верхний край биты, от него мяч стартует и отскакивает.

## Мяч

`BallController`. Движение ручное, без физики: за кадр — шаг `ballSpeed × dt`, один луч вперёд и два
боковых (с паузой `sideRayCooldownFrames`), отражение от нормали.

- Пока мяч прикреплён — висит над битой на высоте `TopEdgeY + ballRadius`.
- Стартует сам через `autoStartDelay`, либо по клику/тапу/пробелу.
- От биты — по точке удара, от стен — зеркально; после отскока добавляется случайное отклонение,
  а почти горизонтальный полёт подталкивается вверх (`maxDeviation`).
- Улетел ниже поля — возврат на биту и новый авто-старт.

## Стены

`FieldBounds` на объекте `Game` каждый кадр ставит `Wall_Left`, `Wall_Right`, `Wall_Top` по числам
конфига. Рендерера у стен нет — они только коллайдеры, поле визуально открыто.

## Фоны

`BackgroundLayer` на каждом слое: бесшовный тайл 1024 × 1024, импорт Sprite/Single, **PPU 10.24**
(один тайл = 100 единиц), Wrap Mode Repeat, Full Rect, без мипов, Max Size 2048.

| Слой | Order | Дрейф, ед./с |
|---|---|---|
| `BG_Far_Nebula` | −20 | 0.35 / 0.10 |
| `BG_Near_Planets` | −10 | 1.10 / 0.30 |

- `tileScale = 2` — один тайл на экране 200 × 200 единиц, `overscanTiles = 1`.
- SpriteRenderer: Draw Mode **Tiled**, Tile Mode **Continuous**; размер = размер кадра + 2 тайла по
  каждой оси, пересчитывается только при смене разрешения.
- Сдвиг идёт по кругу ровно на один тайл (`Mathf.Repeat`), поэтому стыка не видно; дрейф только в
  Play, подгонка размера — и в редакторе.

## Управление

- **Драг мышью / свайп пальцем** — бита.
- **Стрелки, A/D** — бита с клавиатуры.
- **Клик / тап / Space** — запустить мяч.

## Скрипты

`Assets/Scripts` — то, что работает в игре:

| Файл | Назначение |
|---|---|
| `Camera/PlayfieldCamera.cs` | вписывает поле 100 × 170 в экран по меньшей стороне |
| `Config/GameConfig.cs` | ассет параметров; `Resolve()` подставляет значения по умолчанию, если ассет не назначен |
| `Gameplay/PaddleController.cs` | бита: свайп, клавиатура, сглаживание, размер, отскок |
| `Gameplay/BallController.cs` | мяч: старт, лучи, отскоки, возврат |
| `Gameplay/FieldBounds.cs` | стены из конфига |
| `Background/BackgroundLayer.cs` | бесшовный дрейфующий фон |

`AgentScripts` — утилиты редактора, запускаются через CLI, в билд не попадают:

| Файл | Назначение |
|---|---|
| `BuildCameraSetup.cs` | камера + два слоя фона, импорт текстур |
| `BuildPlayfield.cs` | бита, мяч, стены — всё из `GameConfig` |
| `BuildGameConfig.cs` | создаёт конфиг и подключает его к камере, бите, мячу, стенам |
| `AndroidSettings.cs` | PlayerSettings под Android: bundle id, версия, IL2CPP, ARM64, minSdk |
| `ProfileSetup.cs` / `ProfileOverrides.cs` | создание и настройка build-профиля Android |
| `DumpBuildProfile.cs` / `OpenBuildProfiles.cs` | печать и открытие build-профилей |
| `SetGameViewPortrait.cs` | размер Game view «Portrait 1080x2400» |
| `DeviceSimWindow.cs` | открыть окно Device Simulator |
| `ReimportDevice.cs` | переимпорт ассета устройства (самого ассета в проекте нет) |
| `TuneBackground.cs` | масштаб тайла фона и пересчёт размеров |
| `ResetBackgroundOffset.cs` | сброс смещения фонов в ноль |
| `CameraState.cs` | снимок состояния камеры для сверки |
| `ImportBricks.cs` | импорт спрайтов блоков (ждёт `blue_brick_02.png`, которого нет) |

## Пересборка сцены и конфига

Порядок: камера и фоны → поле → конфиг. Каждый скрипт сам сохраняет сцену.

```
unity command script run --file AgentScripts/BuildCameraSetup.cs --entry BuildCameraSetup.Run
unity command script run --file AgentScripts/BuildPlayfield.cs --entry BuildPlayfield.Run
unity command script run --file AgentScripts/BuildGameConfig.cs --entry BuildGameConfig.Run
```

`BuildPlayfield.cs` после переписывания под `GameConfig` проверен только компиляцией: перед первым
запуском сравни его вывод с текущей сценой (позиция биты, старт мяча, размеры стен).

## Сборка APK

Подробно — `BUILD_ANDROID.md`: build-профиль `Assets/Settings/Build Profiles/Android™.asset`,
`com.astra.bricks.test`, «Astra Bricks», версия 0.6 (6), IL2CPP, arm64-v8a, minSdk 26, билды в
`D:\Unity\Astra Bricks\Builds`.

В `BUILD_ANDROID.md` два места уже не соответствуют проекту: имя профиля записано как
`Android.asset` (файл называется `Android™.asset`), а пример строки `[layout] …` в логе устройства
относится к удалённому коду — сейчас никакая диагностика раскладки не пишется.

## Чего пока нет

Расхождения с `..\Docs\Tech\TR_AstraBricks.md` — это фронт работ, а не поломки:

- **блоков и сетки** нет вообще: ни скрипта блока, ни раскладки, ни HUD (TR 1.2);
- **safe area** не учитывается: камера вписывает поле в весь экран (TR 1.3);
- **сортировочный слой `Background`** из TR 2.4 не создан — спрайты живут на `Default`, порядок
  задан через `Sorting Order`;
- `ImportBricks.cs` ссылается на несуществующий `blue_brick_02.png` — в `Assets/Art/Bricks` лежит
  только `blue_brick_01.png`;
- предупреждение `CS0618` в `BallController.cs` (`FindFirstObjectByType` устарел, не критично).
