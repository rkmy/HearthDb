#region

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

#endregion

namespace HearthDb.CardIdGenerator
{
	internal class Program
	{
		private const string Dir = "../../../../HearthDb/";

		static void Main()
		{
			var header = ParseLeadingTrivia(@"/* THIS CLASS WAS GENERATED BY HearthDb.CardIdGenerator. DO NOT EDIT. */");

			var changes = 0;
			var complete = 0;

			var decls = SyntaxBuilder.GetCollectible().Concat(SyntaxBuilder.GetNonCollectible());
			var total = decls.Count();

			Parallel.ForEach(decls, (item) => {
				var (name, decl) = item;
				var cCardIds = ClassDeclaration("CardIds")
					.AddModifiers(Token(PublicKeyword), Token(PartialKeyword))
					.WithLeadingTrivia(header)
					.AddMembers(decl);
				var ns = NamespaceDeclaration(IdentifierName("HearthDb")).AddMembers(cCardIds);
				var fileName = $"CardIds.{name}.cs";
				var path = Dir + fileName;
				Console.WriteLine($"Formatting {fileName} namespace...");
				var root = Formatter.Format(ns, new AdhocWorkspace());
				var rootString = root.ToString();

				string prevString = "";
				if (File.Exists(path))
				{
					using (var sr = new StreamReader(path))
						prevString = sr.ReadToEnd();
				}

				complete++;
				if (prevString != rootString)
				{
					changes++;
					Console.WriteLine($"... {fileName} changed. Writing to disk. ({complete}/{total})");
					using(var sr = new StreamWriter(path))
						sr.Write(root.ToString());
				}
				else
					Console.WriteLine($"... no changes for {fileName}. ({complete}/{total})");
			});

			Console.WriteLine($"{changes} files changed. Press any key to exit.");

			Console.ReadKey();
		}
	}
}
