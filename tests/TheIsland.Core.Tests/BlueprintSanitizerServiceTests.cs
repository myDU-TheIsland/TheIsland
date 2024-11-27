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
        BlueprintSanitizerService service = new BlueprintSanitizerService();

        byte[] bytes = Encoding.Default.GetBytes(_badBpJson);

        NQutils.Config.Config.ReadYamlFile("mod", "./dual.yaml");

        ulong elementType = 1205879482UL;
        WeaponUnit weapunit = new WeaponUnit();

        IGameplayDefinition gameplayDef = Substitute.For<IGameplayDefinition>();
        gameplayDef.BaseObject.Returns(weapunit);
        gameplayDef.GetStaticPropertyOpt(Arg.Is("baseDamage"))
            .Returns(new PropertyValue(10000));

        IGameplayBank bank = Substitute.For<IGameplayBank>();
        bank.GetDefinition(Arg.Is(elementType))
            .Returns(gameplayDef);
        
        bank.GetDefinition(Arg.Any<ulong>())
            .Returns(gameplayDef);

        Assert.DoesNotThrowAsync(async () =>
        {
            BlueprintSanitationResult result = await service.SanitizeAsync(bank, bytes, CancellationToken.None);

            using MemoryStream memoryStream = new MemoryStream(result.BlueprintBytes);
            using StreamReader streamReader = new StreamReader(memoryStream);
            await using JsonTextReader textReader = new JsonTextReader(streamReader);

            JToken bp = await JToken.ReadFromAsync(textReader).ConfigureAwait(false);

            JToken? elements = bp["Elements"];
            Dictionary<ulong, JToken> elementmap = elements !
                .DistinctBy(k => k["elementType"] !.Value<ulong>())
                .ToDictionary(
                    k => k["elementType"] !.Value<ulong>(),
                    v => v);

            JToken? weapprops = elementmap[elementType]["properties"];
            double basedamage = weapprops ![0] ![1] !["value"] !.Value<double>();

            Assert.That(weapunit.baseDamage, Is.EqualTo(basedamage));
        });
    }
}