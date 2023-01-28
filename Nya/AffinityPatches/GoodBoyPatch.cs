using SiraUtil.Affinity;
using TMPro;

namespace Nya.AffinityPatches
{
	internal sealed class GoodBoyPatch : IAffinity
	{
		// The alternative solution to achieve the funny would be a pain to implement, so we're gonna be a bit lazy and do this instead 
		[AffinityPrefix]
		[AffinityPatch(typeof(TMP_Text), nameof(TMP_Text.text), AffinityMethodType.Setter)]
		private void Woof(ref string? value)
		{
			if (value is null)
			{
				return;
			}
			
			if (value == "Nya")
			{
				value = "Woof";
			}
			else if (value.Contains("Nya"))
			{
				value = value.Replace("Nya", "Woof");
			}
		}
	}
}