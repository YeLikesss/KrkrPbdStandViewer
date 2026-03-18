# 操作手册

## 使用准备
* Windows电脑已安装 .Net 6 运行环境
* 游戏已解包并且已还原文件名
* 立绘文件与立绘图层位于同一目录
* 软件不内置TLG解码器, 需转码到指定格式

---

## 格式支持
* 立绘文件<br>
&emsp;pbd
* 输入图像<br>
&emsp;bmp<br>
&emsp;png<br>
&emsp;webp<br>
&emsp;tiff/tif<br>
&emsp;tga<br>
* 输出图像<br>
&emsp;webp<br>
&emsp;png (默认)<br>
&emsp;bmp<br>
&emsp;tga<br>

---

## 界面操作
### 菜单栏
* 文件<br>
&emsp;打开 -> 打开Pbd立绘文件<br>
&emsp;导出<br>
&emsp;&emsp;预览立绘 -> 编码预览窗口内容<br>
&emsp;&emsp;全部立绘 -> 编码立绘合成栈已勾选图层<br>
* 设置<br>
&emsp;导出格式 -> 选择输出图像编码<br>
&emsp;游戏参数 -> 选择游戏Pbd加密参数<br>

### 状态栏
* 依次显示<br>
&emsp;当前立绘名<br>
&emsp;当前画布大小<br>
&emsp;当前输出格式<br>
&emsp;当前进度<br>

### 主窗口
&emsp;1. 预览窗口 -> 根据预览状态与图层级显示<br>
&emsp;2. 预览复选框 -> 勾选启用预览<br>
&emsp;3. 图层信息<br>
&emsp;4. 图层Z轴 -> -/+按钮切换图层级<br>
&emsp;5. 立绘合成栈<br>
&emsp;6. 合成复选框 -> 勾选参与批量合成<br>
&emsp;7. 日志窗口<br>
![主窗口](./Resource/MainWindow_01.png)<br>

---

### 结果输出
* 预览输出<br>
&emsp;工具目录/Preview_Export/<br>
* 合成输出<br>
&emsp;工具目录/Stand_Export/立绘名称/<br>
