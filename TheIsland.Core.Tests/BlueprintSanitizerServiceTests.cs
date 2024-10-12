using System.Text;
using Backend;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NQ;
using NQutils.Def;
using NSubstitute;
using TheIsland.Core.Services.Blueprint;

namespace TheIsland.Core.Tests;

public class BlueprintSanitizerServiceTests
{
    private string _badBpJson;

    [SetUp]
    public void Setup()
    {
        _badBpJson = ResourceLoader.GetStringContents("TheIsland.Core.Tests.Resources.exploit_bp.json");
    }

    [Test]
    public void Should_Sanitize_Blueprint()
    {
        var service = new BlueprintSanitizerService();

        var bytes = Encoding.Default.GetBytes(_badBpJson);

        NQutils.Config.Config.ReadYamlFile("mod", "./dual.yaml");

        var elementType = 1205879482UL;
        var weapunit = new WeaponUnit();
        
        var gameplayDef = Substitute.For<IGameplayDefinition>();
        gameplayDef.BaseObject.Returns(weapunit);
        gameplayDef.GetStaticPropertyOpt(Arg.Is("baseDamage"))
            .Returns(new PropertyValue(10000));
        
        var bank = Substitute.For<IGameplayBank>();
        bank.GetDefinition(Arg.Is(elementType))
            .Returns(gameplayDef);
        
        bank.GetDefinition(Arg.Any<ulong>())
            .Returns(gameplayDef);

        Assert.DoesNotThrowAsync(async () =>
        {
            var result = await service.SanitizeAsync(bank, bytes, CancellationToken.None);

            using var memoryStream = new MemoryStream(result.BlueprintBytes);
            using var streamReader = new StreamReader(memoryStream);
            await using var textReader = new JsonTextReader(streamReader);

            var bp = await JToken.ReadFromAsync(textReader).ConfigureAwait(false);

            var elements = bp["Elements"];
            var elementmap = elements !
                .DistinctBy(k => k["elementType"] !.Value<ulong>())
                .ToDictionary(
                    k => k["elementType"] !.Value<ulong>(),
                    v => v);

            var weapprops = elementmap[elementType]["properties"];
            var basedamage = weapprops ![0] ![1] !["value"] !.Value<double>();

            Assert.That(weapunit.baseDamage, Is.EqualTo(basedamage));
        });
    }
}