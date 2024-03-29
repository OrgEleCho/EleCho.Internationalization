# EleCho.Internationalization

自动生成国际化字符串. \
Automatically generate internationalized strings.


## 使用 / Usage

1. 准备多份语言文件, 格式为 ini \
   Prepare multiple language files in ini format

   ```ini
   Hello=你好
   ```
   
   ```ini
   Hello=こんいちは
   ```

2. 创建一个类, 并添加 `GenerateStrings` 特性, 指定语言代码以及语言文件所在位置: \
   Create a class and add the 'GenerateStrings' attribute to specify the language code and the location of the language file:

   ```csharp
   [GenerateStrings("zh-CN", "/Strings/Chinese.ini")]
   [GenerateStrings("ja-JP", "/Strings/Japanese.ini")]
   public partial class AppStrings
   {
       // members will be auto generated
   }
   ```

3. 类中会自动生成对应的属性, 例如名为 "Hello" 的字符串会生成 "StringHello" 属性.
   当对属性取值时, 会根据其设定的 `CurrentCulture`, 返回对应字符串. \
   The corresponding property is automatically generated in the class, for example, a string named "Hello" will generate a "StringHello" property.
   When a value is taken for a property, the corresponding string will be returned according to the `CurrentCulture` setting.

   ```csharp
   AppStrings appStrings = new();

   appStrings.CurrentCulture = new CultureInfo("zh-CN");
   Console.WriteLine(appStrings.StringHello);    // Output: 你好

   appStrings.CurrentCulture = new CultureInfo("ja-JP");
   Console.WriteLine(appStrings.StringHello);    // Output: こんにちは
   ```

4. 你也可以直接在 WPF 项目中直接将 UI 上的文本内容直接绑定到生成的属性上.
   当 `CurrentCulture` 变更时, 会自动通知属性变更, 所以 UI 也能够自动更新. \
   You can also bind the text content on the UI directly to the generated properties directly in your WPF project.
   When `CurrentCulture` changes, it is automatically notified of property changes, so the UI can be updated automatically.

   ```csharp
   public class TestPageViewModel
   {
       public AppSettings AppSettings { get; } = new();
   }

   public class TestPage : Page
   {
       public TestPage()
       {
           InitializeComponents();

           DataContext = new TestPageViewModel();
       }
   }
   ```

   ```xml
   <TextBlock Text="{Binding AppSettings.StringHello}"/>
   ```