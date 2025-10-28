# SMWP2 资源规范
### 文件名：
- 不要添加前缀（`s_tex.png` -> `tex.png`）
- `tres`后缀的资源加一个二级扩展名来表示资源类型（例如某个 AtlasTexture 类型的资源以`.tex.2d`结尾（AtlasTexture 也是材质的一种））
- 音频流资源以 `bgm`/`bgs`/`me`/`se` 中的一个作为二级后缀，分别用于音乐，环境音，短音乐（死亡，通关等），音效。例：`leisurely_seashore.bgm.ogg`, `mario_died.se.wav`
### 文件结构
除脚本外，其他资源按资源用途划分，放入`content`文件夹或其子文件夹内。<br>
例：马里奥尸体的场景和贴图音效全部放入`res://content/mario/`文件夹内，命名为`mario_corpse.png`和`mario_corpse.tscn`。
### 脚本文件结构
C# 脚本全部放入`scripts`文件夹内，以子命名空间划分，并采用小写+下划线的格式命名文件夹。
例：`SMWP.Level.Data`命名空间的脚本应该放入`res://scripts/level/data`文件夹内。其中`SMWP`为根命名空间，
没有对应的文件夹层级。<br>
GDScript 脚本应被用于完全独立，和其他代码没有交互的地方，并且被视为一种资源文件，和其他资源一起放在`content`文件夹或其对应的子文件夹内。
