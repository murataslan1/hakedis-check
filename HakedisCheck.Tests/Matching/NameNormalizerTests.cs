using HakedisCheck.Core.Matching;

namespace HakedisCheck.Tests.Matching;

public sealed class NameNormalizerTests
{
    [Fact]
    public void Normalize_RewritesTurkishCharactersAndWhitespace()
    {
        var normalized = NameNormalizer.Normalize("  Ayşe   Şağın İleri  ");

        Assert.Equal("AYSE SAGIN ILERI", normalized);
    }
}
