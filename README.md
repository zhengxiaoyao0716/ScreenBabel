# ScreenBabel
Recognize and translate the screen text content.

***
嘛，主要是用来推生肉Gal~

***
## 关于识别模式：
### 有道云 YouDao
需要创建应用实例，绑定服务，获取ID与密钥等：https://ai.youdao.com/doc.s

### Azure
需要分别获取图片识别与翻译的Key等。

图片识别：https://docs.microsoft.com/zh-cn/azure/cognitive-services/computer-vision/vision-api-how-to-topics/howtosubscribe

文本翻译：https://docs.microsoft.com/zh-cn/azure/cognitive-services/translator/translator-text-how-to-signup
（注意，由于没有国际信用卡，Azure的文本翻译权限我没申请到，没做测试）

### Tesseract
呃，本地识别模式暂时关闭了。原因是识别率太低，打包体积又太大。
