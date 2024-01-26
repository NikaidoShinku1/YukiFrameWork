ViewController 字段绑定:

将对象拖入指定位置如图所示:

![输入图片说明](Texture/Bind1.png)

字段的访问权限可在private protected internal public中选择,可以选择字段的类型(从这里绑定的所有字段默认标记SerializeField特性序列化)

绑定好字段后点击生成代码,对象会自动绑定到脚本中,无需手动拖动。最终如图所示:
![输入图片说明](Texture/Bind2.png)

最后会生成该脚本的分写文件
![输入图片说明](Texture/Bind3.png)



