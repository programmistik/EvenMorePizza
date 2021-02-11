using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EvenMorePizza
{
    class Program
    {
        public static string PathToFile { get; set; }

        static void Main(string[] args)
        {
            var exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var thisAppPath = appPathMatcher.Match(exePath).Value;

            string outputPath = Path.Combine(thisAppPath, "OutputFiles");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\a_example.in");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\b_little_bit_of_everything.in");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\c_many_ingredients.in");
            string PathToFile = Path.Combine(thisAppPath, "InputFiles\\d_many_pizzas.in");
            //string PathToFile = Path.Combine(thisAppPath, "InputFiles\\e_many_teams.in");

            Problem p = Problem.LoadProblem(PathToFile);
            Solution solution = BuildMySolution(p);
            // Output
            var pp = outputPath + "\\dd.txt";
            using (StreamWriter sw = new StreamWriter(pp))
            {
                sw.WriteLine(solution.Deliveries.Count);
                foreach (Delivery delivery in solution.Deliveries)
                {
                    sw.Write(delivery.DeliveryPizzas.Count);
                    foreach (Pizza pizza in delivery.DeliveryPizzas)
                    {
                        sw.Write(' ');
                        sw.Write(pizza.id);
                    }
                    sw.WriteLine();
                }
            }

            Console.WriteLine("Done!");

            Console.ReadKey();
        }
        public static List<Pizza> CreateDelivery(List<Pizza> pizzas, int DeliverySize)
        {
            var currentDeliveryIngredients = new HashSet<string>();
            var currentDelivery = new List<Pizza>();

            if (pizzas.Count > 0)
            {
                currentDelivery.Add(pizzas[0]);
                pizzas.RemoveAt(0);
            }

            foreach (Pizza pizza in currentDelivery)
                currentDeliveryIngredients.UnionWith(pizza.Ingredients);

            while (currentDelivery.Count < DeliverySize)
            {
                int nextPizzaIndex = -1;
                int nextPizzaOverlapIngredientsCount = 0;
                int nextPizzaNewIngredientsCount = 0;

                for (int i = 0; i < pizzas.Count; i++)
                {
                    if (pizzas[i].count < nextPizzaNewIngredientsCount)
                        break;

                    int overlapIngredientsCount = GetOverlapCount(currentDeliveryIngredients, pizzas[i].Ingredients);
                    int newIngredientsCount = pizzas[i].count - overlapIngredientsCount;

                    if ((newIngredientsCount > nextPizzaNewIngredientsCount)
                        ||
                        ((newIngredientsCount == nextPizzaNewIngredientsCount) && (overlapIngredientsCount < nextPizzaOverlapIngredientsCount)) )
                    {
                        nextPizzaIndex = i;
                        nextPizzaOverlapIngredientsCount = overlapIngredientsCount;
                        nextPizzaNewIngredientsCount = newIngredientsCount;

                        if (nextPizzaOverlapIngredientsCount == 0)
                            break;
                    }
                }

                // Не осталось лучших пицц
                if (nextPizzaNewIngredientsCount == 0)
                    break;

                currentDelivery.Add(pizzas[nextPizzaIndex]);
                currentDeliveryIngredients.UnionWith(pizzas[nextPizzaIndex].Ingredients);
                pizzas.RemoveAt(nextPizzaIndex);
            }

            return currentDelivery;
        }

        public static int GetOverlapCount<T>(HashSet<T> a, HashSet<T> b)
        {
            HashSet<T> small;
            HashSet<T> big;
            if (a.Count < b.Count)
            {
                small = a;
                big = b;
            }
            else
            {
                small = b;
                big = a;
            }

            int size = 0;
            
            foreach (T val in small)
                if (big.Contains(val))
                    size++;

            return size; // количество общих ингредиентов
        }
        
        public static Solution BuildMySolution(Problem p)
        {
            var pizzas = p.Pizzas.OrderByDescending(x => x.count).ToList();
            var deliveries = new List<Delivery>();
            var teamSizes = new int[] { 0, 0, p.Team2, p.Team3, p.Team4 };

            while (pizzas.Count >= 2)
            {
                int maxDeliverySize = 0;
                for (int i = 4; i >= 2; i--)
                {
                    if (teamSizes[i] > 0)
                    {
                        maxDeliverySize = i;
                        break;
                    }
                }

                if (maxDeliverySize == 0)
                    break;

                List<Pizza> currentDelivery = CreateDelivery(pizzas, maxDeliverySize);

                if (teamSizes[currentDelivery.Count] == 0)
                {
                    while (pizzas.Count > 0)
                    {
                        int nextPizza = pizzas.Count - 1;
                        currentDelivery.Add(pizzas[nextPizza]);
                        pizzas.RemoveAt(nextPizza);

                        if (teamSizes[currentDelivery.Count] > 0)
                            break;
                    }
                }

                teamSizes[currentDelivery.Count]--;
                deliveries.Add(new Delivery(currentDelivery));
            }

            return new Solution(deliveries, pizzas);
        }
    }

    /// CLASSES
    /// 
    class Pizza
    {
        public int id { get; set; }
        public int count { get; set; }
        public HashSet<string> Ingredients { get; set; }

        public Pizza(int id, HashSet<string> ingredients, int ingCount)
        {
            this.id = id;
            Ingredients = ingredients;
            count = ingCount;
        }

    }

    class Solution
    {
        public List<Delivery> Deliveries { get; set; }
        public List<Pizza> UnusedPizzas { get; set; }

        public Solution(List<Delivery> deliveries, List<Pizza> unusedPizzas)
        {
            Deliveries = deliveries;
            UnusedPizzas = unusedPizzas;
        }
           
    }

    class Delivery
    {
        public List<Pizza> DeliveryPizzas { get; set; }

        public Delivery(List<Pizza> pizzas)
        {
            DeliveryPizzas = new List<Pizza>(pizzas);
        }
       
    }

    class Problem
    {
        public int Team2 { get; set; }
        public int Team3 { get; set; }
        public int Team4 { get; set; }
        public List<Pizza> Pizzas { get; set; }
        public Dictionary<string, List<Pizza>> IngInPizzas { get; set; }

        private Problem(int t2, int t3, int t4, List<Pizza> pizzas)
        {
            Pizzas = pizzas;
            Team2 = t2;
            Team3 = t3;
            Team4 = t4;

            IngInPizzas = new Dictionary<string, List<Pizza>>();
            foreach (Pizza p in pizzas)
                foreach (var ingredient in p.Ingredients)
                {
                    List<Pizza> ingredientPizzas;
                    if (!IngInPizzas.TryGetValue(ingredient, out ingredientPizzas))
                    {
                        ingredientPizzas = new List<Pizza>();
                        IngInPizzas.Add(ingredient, ingredientPizzas);
                    }
                    ingredientPizzas.Add(p);
                }
        }

        public static Problem LoadProblem(string fileName)
        {           

            using (StreamReader sr = new StreamReader(fileName))
            {
                var line = sr.ReadLine();
                var Parts = line.Split(' ');
                int PizzaCount = int.Parse(Parts[0]);
                int Team2 = int.Parse(Parts[1]);
                int Team3 = int.Parse(Parts[2]);
                int Team4 = int.Parse(Parts[3]);
                var pizzas = new List<Pizza>();
             //   var IngMap = new Dictionary<string, int>();
              //  var NextIngId = 0;

                for (int k = 0; k < PizzaCount; k++)
                {
                    line = sr.ReadLine();
                    Parts = line.Split(' ');
                    var ingCount = int.Parse(Parts[0]);
                    var ingredientsInPizza = new HashSet<string>();

                    for (int i = 1; i <= ingCount; i++)
                    {
                        var ing = Parts[i];
                        //if (!IngMap.ContainsKey(ing))
                        //    IngMap.Add(ing, NextIngId++);

                        //ingredientsInPizza.Add(IngMap[ing]);
                        
                    }

                    pizzas.Add(new Pizza(k, ingredientsInPizza, ingCount));

                
                }

                return new Problem(Team2, Team3, Team4, pizzas);
            }
        }
    }
}
