namespace AsitLib.Tests
{
    [TestClass]
    public class TypeDictionaryTests
    {
        public interface IAnimal { }
        public class Dog : IAnimal { }
        public class Cat : IAnimal { }
        public class GoldenRetriever : Dog { }

        [TestMethod]
        public void Add_ImplementingType()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();
            Dog dog = new Dog();

            dictionary.Add<Dog>(dog);

            dictionary.Should().HaveCount(1);
            dictionary.ContainsKey(typeof(Dog)).Should().BeTrue();
            dictionary[typeof(Dog)].Should().BeSameAs(dog);
        }

        [TestMethod]
        public void GetValue_TypeInDictionary_GetsValueWithTypeAsKey()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();
            Dog dog = new Dog();

            dictionary.Add<Dog>(dog);

            dictionary.GetValue<Dog>().Should().BeSameAs(dog);
        }

        [TestMethod]
        public void ContainsType_ParentClassThatsNotAdded_ReturnsFalse()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();
            dictionary.Add<Dog>(new Dog());

            dictionary.ContainsType<IAnimal>().Should().BeFalse();
        }

        [TestMethod]
        public void ContainsType_AddedType_ReturnsTrue()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();
            dictionary.Add<Dog>(new Dog());

            dictionary.ContainsType<Dog>().Should().BeTrue();
        }

        [TestMethod]
        public void Add_DerivedType_OnlyAddsForExactType()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();
            GoldenRetriever golden = new GoldenRetriever();

            dictionary.Add<GoldenRetriever>(golden);

            dictionary.ContainsKey(typeof(GoldenRetriever)).Should().BeTrue();
            dictionary.ContainsKey(typeof(IAnimal)).Should().BeFalse();
            dictionary.ContainsKey(typeof(Dog)).Should().BeFalse();
        }

        [TestMethod]
        public void Add_MultipleDifferentTypes()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();
            Dog dog = new Dog();
            Cat cat = new Cat();

            dictionary.Add<Dog>(dog);
            dictionary.Add<Cat>(cat);

            dictionary.Should().HaveCount(2);
            dictionary.ContainsKey(typeof(Dog)).Should().BeTrue();
            dictionary.ContainsKey(typeof(Cat)).Should().BeTrue();
            dictionary[typeof(Dog)].Should().BeSameAs(dog);
            dictionary[typeof(Cat)].Should().BeSameAs(cat);
        }

        [TestMethod]
        public void Add_Duplicate_ThrowsEx()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();
            Dog dog1 = new Dog();
            Dog dog2 = new Dog();

            dictionary.Add<Dog>(dog1);

            Invoking(() => dictionary.Add<Dog>(dog2)).Should().Throw<ArgumentException>();
        }

        //[TestMethod]
        //public void Add_TypeNotAssignableToParentType_ThrowsEx()
        //{
        //    TypeDictionary<Dog> dictionary = new TypeDictionary<Dog>();
        //    Dog dog = new Dog();
        //    GoldenRetriever golden = new GoldenRetriever();
        //    Cat cat = new Cat();

        //    dictionary.Add(dog);
        //    dictionary.Add(golden);

        //    Invoking(() => dictionary.Add(cat.GetType(), cat)).Should().Throw<ArgumentException>();
        //}

        [TestMethod]
        public void Add_Null_ThrowsEx()
        {
            TypeDictionary<IAnimal> dictionary = new TypeDictionary<IAnimal>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Invoking(() => dictionary.Add<Dog>(null)).Should().Throw<ArgumentNullException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}