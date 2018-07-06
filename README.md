# ScreenBabel
Recognize and translate the screen text content.

***
## 嘛，主要是用来推生肉Gal~
效果预览：
![preview](https://user-images.githubusercontent.com/12682063/42393359-bba191b8-8188-11e8-8a63-5993504bfbc7.png)

折叠标题栏：
![image](https://user-images.githubusercontent.com/12682063/42393437-f48e1186-8188-11e8-98a6-2b35e81534af.png)

设置：
![image](https://user-images.githubusercontent.com/12682063/42393512-2ebac43a-8189-11e8-8924-81c9b7527cf7.png)


***
## 关于识别模式：
### 有道云 YouDao
需要创建应用实例，绑定服务，获取ID与密钥等：https://ai.youdao.com/doc.s

### 微软 Azure
需要分别获取图片识别与翻译的Key等。

图片识别：https://docs.microsoft.com/zh-cn/azure/cognitive-services/computer-vision/vision-api-how-to-topics/howtosubscribe

文本翻译：https://docs.microsoft.com/zh-cn/azure/cognitive-services/translator/translator-text-how-to-signup
（注意，由于没有国际信用卡，Azure的文本翻译权限我没申请到，没做测试）

### 本地 Tesseract
呃，本地识别模式暂时关闭了。原因是识别率太低，打包体积又太大。
