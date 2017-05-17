// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 08 10 2014 at 13:37 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable JoinDeclarationAndInitializer
namespace Ophidian.Losgap {
	public partial class ExtensionsTest {
		#region Tests
		[TestMethod]
		public void TestForEach() {
			// Define variables and constants
			StringBuilder stringBuilder = new StringBuilder();
			int[] intArray = {
				1, 2, 3, 4, 5, 6, 7, 8, 9
			};
			string[] stringArray = {
				"Do test",
				"????",
				"Profit!"
			};

			// Set up context


			// Execute
			intArray.ForEach(i => stringBuilder.Append(i));
			stringArray.ForEach((str, i) => stringArray[i] = (i + 1) + ": " + str);

			// Assert outcome
			Assert.AreEqual("123456789", stringBuilder.ToString());
			Assert.AreEqual("1: Do test", stringArray[0]);
			Assert.AreEqual("2: ????", stringArray[1]);
			Assert.AreEqual("3: Profit!", stringArray[2]);
		}

		[TestMethod]
		public void TestCountInstances() {
			// Define variables and constants
			string[] animals = {
				"Dog", "Dog", "Dog", "Dog",
				"Cat", "Cat",
				"Lizard", "Lizard", "Lizard", "Lizard", "Lizard", "Lizard",
			};

			// Set up context


			// Execute
			IDictionary<string, int> instanceCount = animals.CountInstances();

			// Assert outcome
			Assert.AreEqual(3, instanceCount.Count);
			Assert.AreEqual(4, instanceCount["Dog"]);
			Assert.AreEqual(2, instanceCount["Cat"]);
			Assert.AreEqual(6, instanceCount["Lizard"]);
		}

		[TestMethod]
		public void TestToStringOfContents() {
			// Define variables and constants
			string[] animals = {
				"Dog", "Dog", "Dog", "Dog",
				"Cat", "Cat",
				"Lizard", "Lizard", "Lizard", "Lizard", "Lizard", "Lizard"
			};

			// Set up context


			// Execute
			string defaultToString = animals.ToStringOfContents();
			string sillyToString = animals.ToStringOfContents(ele => {
				switch (ele) {
					case "Dog":
						return "Woof!";
					case "Cat":
						return "Miaow.";
					default:
						return "Meh";
				}
			});

			// Assert outcome
			Assert.AreEqual("{Dog x4, Cat x2, Lizard x6}", defaultToString);
			Assert.AreEqual("{Woof! x4, Miaow. x2, Meh x6}", sillyToString);
		}

		[TestMethod]
		public void TestSubsequence() {
			// Define variables and constants
			string[] animals = {
				"Dog", "Dog", "Dog", "Dog",
				"Cat", "Cat",
				"Lizard", "Lizard", "Lizard", "Lizard", "Lizard", "Lizard"
			};

			// Set up context


			// Execute
			IEnumerable<string> justDogs = animals.Subsequence(lastElementIndexEx: 4);
			IEnumerable<string> justCats = animals.Subsequence(4, 6);
			IEnumerable<string> justLizards = animals.Subsequence(firstElementIndexInc: 6);

			// Assert outcome
			Assert.IsTrue(justDogs.Count() == 4);
			Assert.IsTrue(justDogs.All(ele => ele == "Dog"));
			Assert.IsTrue(justCats.Count() == 2);
			Assert.IsTrue(justCats.All(ele => ele == "Cat"));
			Assert.IsTrue(justLizards.Count() == 6);
			Assert.IsTrue(justLizards.All(ele => ele == "Lizard"));
		}

		[TestMethod]
		public void TestRelationship() {
			// Define variables and constants
			int[] numbers = {
				2, 6, 540, 18, 54	
			};

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsFalse(numbers.AnyRelationship((a, b) => a / b == 2));
			Assert.IsTrue(numbers.AnyRelationship((a, b) => a / b == 10));
			Assert.AreEqual(3, numbers.CountRelationship((a, b) => a / b == 3));
			Assert.AreEqual(0, numbers.CountRelationship((a, b) => a / b == 2));
			Assert.AreEqual(1, numbers.CountRelationship((a, b) => a / b == 10));

			Assert.IsNull(numbers.FirstRelationship((a, b) => a / b == 2));
			Assert.AreEqual(new KeyValuePair<int, int>(540, 54), numbers.FirstRelationship((a, b) => a / b == 10));

			var whereA = numbers.WhereRelationship((a, b) => a / b == 3);
			var whereB = numbers.WhereRelationship((a, b) => a / b == 2);
			var whereC = numbers.WhereRelationship((a, b) => a / b == 10);

			Assert.AreEqual(new KeyValuePair<int, int>(2, 6), whereA.ElementAt(0));
			Assert.AreEqual(new KeyValuePair<int, int>(6, 18), whereA.ElementAt(1));
			Assert.AreEqual(new KeyValuePair<int, int>(18, 54), whereA.ElementAt(2));
			Assert.AreEqual(0, whereB.Count());
			Assert.AreEqual(new KeyValuePair<int, int>(540, 54), whereC.Single());
		}

		[TestMethod]
		public void TestAnyDuplicates() {
			// Define variables and constants
			int[] numbers = {
				1, 2, 3, 4, 5, 6, 1,
			};

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(numbers.AnyDuplicates());
			Assert.IsFalse(numbers.Skip(1).AnyDuplicates());
		}

		[TestMethod]
		public void TestAtLeast() {
			// Define variables and constants
			string[] animals = {
				"Dog", "Dog", "Dog", "Dog",
				"Cat", "Cat",
				"Lizard", "Lizard", "Lizard", "Lizard", "Lizard", "Lizard"
			};

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(animals.AtLeast(1, animal => animal == "Cat"));
			Assert.IsTrue(animals.AtLeast(4, animal => animal == "Dog"));
			Assert.IsTrue(animals.AtLeast(0, animal => animal == "Elephant"));
			Assert.IsFalse(animals.AtLeast(7, animal => animal == "Lizard"));
		}

		[TestMethod]
		public void TestAtMost() {
			// Define variables and constants
			string[] animals = {
				"Dog", "Dog", "Dog", "Dog",
				"Cat", "Cat",
				"Lizard", "Lizard", "Lizard", "Lizard", "Lizard", "Lizard"
			};

			// Set up context


			// Execute


			// Assert outcome
			Assert.IsTrue(animals.AtMost(2, animal => animal == "Cat"));
			Assert.IsTrue(animals.AtMost(4, animal => animal == "Dog"));
			Assert.IsTrue(animals.AtMost(0, animal => animal == "Elephant"));
			Assert.IsFalse(animals.AtMost(5, animal => animal == "Lizard"));
		}

		[TestMethod]
		public void TestRemoveWhere() {
			// Define variables and constants
			string[] animals = {
				"Dog", "Dog", "Dog", "Dog",
				"Cat", "Cat",
				"Lizard", "Lizard", "Lizard", "Lizard", "Lizard", "Lizard"
			};

			// Set up context
			IList<string> mutableAnimalsCollection = animals.ToList();

			// Execute
			mutableAnimalsCollection.RemoveWhere(animal => animal.Contains("a"));

			// Assert outcome
			foreach (string animal in mutableAnimalsCollection) {
				Assert.AreEqual("Dog", animal);
			}
		}

		[TestMethod]
		public void TestFlatten() {
			// Define variables and constants
			string[][] animalsGroups = {
				new[] { "Dog", "Cat", "Elephant", "Whale" },
				new[] { "Lizard", "Snake", "Tortoise", "Crocodile" },
				new[] { "Tit", "Jay", "Hawk", "Pterodactyl" }
			};

			// Set up context


			// Execute
			IEnumerable<string> flattenedAnimals = animalsGroups.Flatten(); // Haha, flattened animals. Also is a pterodactyl a bird??

			// Assert outcome
			int index = 0;
			Assert.AreEqual("Dog", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Cat", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Elephant", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Whale", flattenedAnimals.ElementAt(index++));

			Assert.AreEqual("Lizard", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Snake", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Tortoise", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Crocodile", flattenedAnimals.ElementAt(index++));

			Assert.AreEqual("Tit", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Jay", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Hawk", flattenedAnimals.ElementAt(index++));
			Assert.AreEqual("Pterodactyl", flattenedAnimals.ElementAt(index++));
		}

		[TestMethod]
		public void TestGetOrCreate() {
			// Define variables and constants
			IDictionary<string, string> dict = new Dictionary<string, string>();

			// Set up context
			dict["A"] = "1";

			// Execute


			// Assert outcome
			Assert.AreEqual("1", dict.GetOrCreate("A", k => "1"));
			Assert.AreEqual("1", dict.GetOrCreate("A", k => "2"));
			Assert.AreEqual("3", dict.GetOrCreate("B", k => "3"));
			Assert.AreEqual("3", dict.GetOrCreate("B", k => "3"));
			Assert.AreEqual("3", dict["B"]);
		}

		[TestMethod]
		public void TestConcat() {
			IEnumerable<string> zoo = new[] { "Dog", "Cat", "Elephant", "Lizard", "Snake" };

			zoo = zoo.Concat("Dragon");

			Assert.AreEqual(6, zoo.Count());
			Assert.AreEqual("Dog", zoo.ElementAt(0));
			Assert.AreEqual("Cat", zoo.ElementAt(1));
			Assert.AreEqual("Elephant", zoo.ElementAt(2));
			Assert.AreEqual("Lizard", zoo.ElementAt(3));
			Assert.AreEqual("Snake", zoo.ElementAt(4));
			Assert.AreEqual("Dragon", zoo.ElementAt(5));

			zoo = zoo.Concat("Octopus", "Seahorse", "Pterodactyl");

			Assert.AreEqual(9, zoo.Count());
			Assert.AreEqual("Dog", zoo.ElementAt(0));
			Assert.AreEqual("Cat", zoo.ElementAt(1));
			Assert.AreEqual("Elephant", zoo.ElementAt(2));
			Assert.AreEqual("Lizard", zoo.ElementAt(3));
			Assert.AreEqual("Snake", zoo.ElementAt(4));
			Assert.AreEqual("Dragon", zoo.ElementAt(5));
			Assert.AreEqual("Octopus", zoo.ElementAt(6));
			Assert.AreEqual("Seahorse", zoo.ElementAt(7));
			Assert.AreEqual("Pterodactyl", zoo.ElementAt(8));
		}

		[TestMethod]
		public void TestExcept() {
			IEnumerable<string> zoo = new[] { "Dog", "Cat", "Elephant", "Lizard", "Snake", "Dragon", "Octopus", "Seahorse", "Pterodactyl" };

			zoo = zoo.Except("Dragon");

			Assert.AreEqual(8, zoo.Count());
			Assert.AreEqual("Dog", zoo.ElementAt(0));
			Assert.AreEqual("Cat", zoo.ElementAt(1));
			Assert.AreEqual("Elephant", zoo.ElementAt(2));
			Assert.AreEqual("Lizard", zoo.ElementAt(3));
			Assert.AreEqual("Snake", zoo.ElementAt(4));
			Assert.AreEqual("Octopus", zoo.ElementAt(5));
			Assert.AreEqual("Seahorse", zoo.ElementAt(6));
			Assert.AreEqual("Pterodactyl", zoo.ElementAt(7));

			zoo = zoo.Except("Dog", "Seahorse", "Pterodactyl");

			Assert.AreEqual(5, zoo.Count());
			Assert.AreEqual("Cat", zoo.ElementAt(0));
			Assert.AreEqual("Elephant", zoo.ElementAt(1));
			Assert.AreEqual("Lizard", zoo.ElementAt(2));
			Assert.AreEqual("Snake", zoo.ElementAt(3));
			Assert.AreEqual("Octopus", zoo.ElementAt(4));
		}
		#endregion
	}
}