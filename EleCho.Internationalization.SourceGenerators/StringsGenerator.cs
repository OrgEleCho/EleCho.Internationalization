using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EleCho.Internationalization.SourceGenerators
{

    [Generator(LanguageNames.CSharp)]
    public class StringsGenerator : IIncrementalGenerator
    {
        record struct ClassStringsGeneration(string ClassName, string ClassNamespace, IEnumerable<StringsGenerationDeclaration> Generations);
        record struct StringsGenerationDeclaration(CultureInfo Culture, string ResourcePath);


        private static IDictionary<string, string> ReadResource(string resourcePath, string? projectDir)
        {
            if (projectDir is not null)
                resourcePath = System.IO.Path.Combine(projectDir, resourcePath);

            if (System.IO.Path.GetExtension(resourcePath).Equals(".ini", StringComparison.OrdinalIgnoreCase))
                return ReadIniResource(resourcePath);
            else
                return new Dictionary<string, string>();

            static IDictionary<string, string> ReadIniResource(string resourcePath)
            {
                var strings = new Dictionary<string, string>();

                string[] lines = System.IO.File.ReadAllLines(resourcePath);
                foreach (var line in lines)
                {
                    int eqIndex = line.IndexOf('=');
                    if (eqIndex < 0)
                        continue;

                    string name = line.Substring(0, eqIndex);
                    string value = line.Substring(eqIndex + 1);

                    if (name.All(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_'))
                        strings.Add(name, value);
                }

                return strings;
            }

            //static IDictionary<string, string> ReadJsonResource()
        }

        private string CorrectString(string str)
        {
            bool escape = false;
            StringBuilder sb = new();
            foreach (var c in str)
            {
                if (escape)
                {
                    sb.Append('\\');
                    sb.Append(c);
                }
                else
                {
                    if (c == '\\')
                    {
                        escape = true;
                    }
                    else if (c == '\"')
                    {
                        // ignore
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var generationProvider = context.SyntaxProvider.ForAttributeWithMetadataName(typeof(GenerateStringsAttribute).FullName, (node, token) => node is ClassDeclarationSyntax, (syntaxContext, token) =>
            {
                if (syntaxContext.TargetSymbol is not ITypeSymbol typeSymbol)
                    throw new Exception($"Invalid attribute owner, symbol name: {syntaxContext.TargetSymbol.Name}");

                string className = typeSymbol.Name;
                string classNamespace = typeSymbol.ContainingNamespace?.Name ?? string.Empty;

                if (string.IsNullOrWhiteSpace(classNamespace))
                    throw new Exception($"Cannot find namespace of symbol, symbol name: {syntaxContext.TargetSymbol.Name}");

                List<StringsGenerationDeclaration> generations = new();
                foreach (var attribute in syntaxContext.Attributes)
                {
                    if (attribute.AttributeClass?.Name != typeof(GenerateStringsAttribute).Name||
                        attribute.AttributeClass?.ContainingNamespace?.ToString() != typeof(GenerateStringsAttribute).Namespace)
                        continue;

                    var arguments = attribute.ConstructorArguments
                        .Select(v => v.Value)
                        .ToArray();

                    if (arguments.Length != 2 ||
                        arguments[0] is not string languageCode ||
                        arguments[1] is not string resourcePath)
                        throw new Exception($"Invalid attribute constructor arguments, class name: {className}");

                    try
                    {
                        generations.Add(new StringsGenerationDeclaration(new CultureInfo(languageCode), resourcePath));
                    }
                    catch
                    {
                        throw new Exception($"Invalid langauge code for strings generation: {languageCode}, class name: {className}");
                    }
                }

                return new ClassStringsGeneration(className, classNamespace, generations);
            });

            var optionsWithGenerationsProvider = context.AnalyzerConfigOptionsProvider.Combine(generationProvider.Collect());

            context.RegisterImplementationSourceOutput(optionsWithGenerationsProvider, (productionContext, optionsWithGenerations) =>
            {
                var options = optionsWithGenerations.Left;

                options.GlobalOptions.TryGetValue("build_property.projectdir", out string? projectDir);

                foreach (var generation in optionsWithGenerations.Right)
                {
                    string[] allStringNames = generation.Generations
                        .SelectMany(g => ReadResource(g.ResourcePath, projectDir).Keys)
                        .Distinct()
                        .ToArray();

                    string source =
                        $$$"""                  
                        #pragma warning disable
                        #nullable enable

                        namespace {{{generation.ClassNamespace}}}
                        {
                            partial class {{{generation.ClassName}}} : 
                                global::EleCho.Internationalization.{{{nameof(II18nStrings)}}}, 
                                global::System.ComponentModel.INotifyPropertyChanging, 
                                global::System.ComponentModel.INotifyPropertyChanged
                            {
                                class _I18nCultureStrings
                                {
                                    public _I18nCultureStrings(global::System.Globalization.CultureInfo culture)
                                    {
                                        Culture = culture;
                                    }

                                    public global::System.Globalization.CultureInfo Culture { get; }
                                    public global::System.Collections.Generic.Dictionary<string, string> Strings { get; } = new();
                                }

                                private List<_I18nCultureStrings> _allI18nStrings = new()
                                {
                        {{{
                                    string.Join(Environment.NewLine, generation.Generations.Select(g =>
                        $$$$"""
                                    new _I18nCultureStrings(new global::System.Globalization.CultureInfo("{{{{g.Culture}}}}"))
                                    {
                                        Strings =
                                        {
                        {{{{
                                            string.Join(Environment.NewLine, ReadResource(g.ResourcePath, projectDir).Select(kv =>
                        $$$$$"""
                                            ["{{{{{kv.Key}}}}}"] = "{{{{{CorrectString(kv.Value)}}}}}",
                        """
                                            ))
                        }}}}
                                        }
                                    },
                        """
                                    ))
                        }}}
                                };

                                private int _currentCultureStringsIndex = -1;
                                private global::System.Globalization.CultureInfo _currentCulture = global::System.Globalization.CultureInfo.CurrentCulture;

                                protected virtual void OnPropertyChanging(string propertyName)
                                {
                                    PropertyChanging?.Invoke(this, new global::System.ComponentModel.PropertyChangingEventArgs(propertyName));
                                }

                                protected virtual void OnPropertyChanged(string propertyName)
                                {
                                    PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
                                }
                        
                                public string GetString(string name)
                                {
                                    if (_allI18nStrings.Count == 0)
                                        return name;
                        
                                    if (_currentCultureStringsIndex < 0)
                                        _currentCultureStringsIndex = _allI18nStrings.FindIndex(i18nStrings => i18nStrings.Culture.Equals(_currentCulture));
                        
                                    if (_currentCultureStringsIndex < 0 && AllowFuzzyMatching)
                                        _currentCultureStringsIndex = _allI18nStrings.FindIndex(i18nStrings => i18nStrings.Culture.TwoLetterISOLanguageName == _currentCulture.TwoLetterISOLanguageName);
                        
                                    if (_currentCultureStringsIndex < 0 && AllowFallback)
                                        _currentCultureStringsIndex = 0;
                        
                                    if (_currentCultureStringsIndex >= 0)
                                        if (_allI18nStrings[_currentCultureStringsIndex].Strings.TryGetValue(name, out string exactValue))
                                            return exactValue;
                        
                                    if (AllowFallback)
                                    {
                                        if (_allI18nStrings[0].Strings.TryGetValue(name, out string fallbackValue))
                                            return fallbackValue;
                        
                                        foreach (var i18nString in _allI18nStrings)
                                            if (i18nString.Strings.TryGetValue(name, out var fallbackValue2))
                                                return fallbackValue2;
                                    }
                        
                                    return name;
                                }

                                public event global::System.ComponentModel.PropertyChangingEventHandler? PropertyChanging;
                                public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

                                public bool AllowFuzzyMatching { get; set; } = false;
                                public bool AllowFallback { get; set; } = false;

                                public global::System.Globalization.CultureInfo CurrentCulture
                                {
                                    get => _currentCulture;
                                    set
                                    {
                                        if (value is null)
                                            throw new ArgumentNullException(nameof(value));
                                        if (value == _currentCulture)
                                            return;
                                            
                        {{{
                                string.Join(Environment.NewLine, allStringNames.Select(name =>
                        $$$$"""
                                        OnPropertyChanging(nameof(String{{{{name}}}}));
                        """
                                ))
                        }}}
                                        OnPropertyChanging(nameof(CurrentCulture));

                                        _currentCulture = value;
                                        _currentCultureStringsIndex = -1;
                                        
                        {{{
                                string.Join(Environment.NewLine, allStringNames.Select(name =>
                        $$$$"""
                                        OnPropertyChanged(nameof(String{{{{name}}}}));
                        """
                                ))
                        }}}
                                        OnPropertyChanged(nameof(CurrentCulture));
                                    }
                                }

                                public IReadOnlyList<global::System.Globalization.CultureInfo> AllCultures { get; } = new List<global::System.Globalization.CultureInfo>()
                                {             
                        {{{
                                string.Join(Environment.NewLine, generation.Generations.Select(g =>
                        $$$$"""
                                    new global::System.Globalization.CultureInfo("{{{{g.Culture}}}}"),
                        """
                                ))
                        }}}
                                }.AsReadOnly();
                                

                                public IReadOnlyList<string> AllStringNames { get; } = new List<string>()
                                {             
                        {{{
                                string.Join(Environment.NewLine, allStringNames.Select(name =>
                        $$$$"""
                                    "{{{{name}}}}",
                        """
                                ))
                        }}}
                                }.AsReadOnly();

                        {{{
                                string.Join(Environment.NewLine, allStringNames.Select(name =>
                        $$$$"""
                                public string String{{{{name}}}} => GetString("{{{{name}}}}");
                        """
                                ))
                        }}}
                            }
                        }
                        """;

                    productionContext.AddSource($"{generation.ClassName}.g.cs", source);
                }

            });
        }
    }
}
