using System; using System.Collections.Generic; using BattleSimulator.Core; using BattleSimulator.Core.Models; using BattleSimulator.Core.Models.Characters;

namespace BattleSimulator.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===== 战斗模拟系统演示 =====");
            Console.WriteLine();
            
            // 创建我方队伍
            List<Entity> allies = CreateAllies();
            
            // 创建敌方队伍
            List<Entity> enemies = CreateEnemies();
            
            // 显示战斗开始信息
            Console.WriteLine("战斗即将开始！");
            Console.WriteLine("我方队伍：");
            foreach (var ally in allies)
            {
                Console.WriteLine($"- {ally.Name} (生命: {ally.CurrentHealth}/{ally.MaxHealth}, 攻击: {ally.Attack}, 防御: {ally.Defense})");
            }
            
            Console.WriteLine("敌方队伍：");
            foreach (var enemy in enemies)
            {
                Console.WriteLine($"- {enemy.Name} (生命: {enemy.CurrentHealth}/{enemy.MaxHealth}, 攻击: {enemy.Attack}, 防御: {enemy.Defense})");
            }
            
            Console.WriteLine();
            Console.WriteLine("按任意键开始战斗...");
            Console.ReadKey();
            Console.WriteLine();
            
            // 创建并初始化战斗系统
            BattleSystem battleSystem = new BattleSystem();
            battleSystem.InitializeBattle(allies, enemies);
            
            // 执行战斗直到结束
            while (!battleSystem.IsBattleOver)
            {
                battleSystem.ExecuteRound();
                
                // 显示当前回合的战斗日志
                DisplayBattleLog(allies[0].BattleLog); // 使用第一个我方角色的战斗日志
                
                // 显示当前状态
                DisplayCurrentStatus(allies, enemies);
                
                Console.WriteLine("按任意键继续下一回合...");
                Console.ReadKey();
                Console.WriteLine();
            }
            
            // 显示战斗结果
            DisplayBattleResult(battleSystem.BattleResult);
            
            Console.WriteLine("战斗模拟结束！");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
        
        /// <summary>
        /// 创建我方队伍
        /// </summary>
        private static List<Entity> CreateAllies()
        {
            List<Entity> allies = new List<Entity>();
            
            // 创建一个战士
            Warrior warrior = new Warrior { Name = "勇敢的亚瑟" };
            allies.Add(warrior);
            
            // 创建一个法师
            Mage mage = new Mage { Name = "智慧的甘道夫" };
            allies.Add(mage);
            
            return allies;
        }
        
        /// <summary>
        /// 创建敌方队伍
        /// </summary>
        private static List<Entity> CreateEnemies()
        {
            List<Entity> enemies = new List<Entity>();
            
            // 创建一个敌方战士
            Entity enemyWarrior = new Entity
            {
                Name = "黑暗骑士",
                MaxHealth = 140,
                Health = 140,
                Attack = 28,
                Defense = 18,
                Speed = 14,
                HitRate = 85,
                DodgeRate = 12,
                CritRate = 12,
                CritDamage = 45,
                Faction = Faction.Enemy
            };
            enemies.Add(enemyWarrior);
            
            // 创建一个敌方法师
            Entity enemyMage = new Entity
            {
                Name = "暗影巫师",
                MaxHealth = 90,
                Health = 90,
                Attack = 35,
                Defense = 8,
                Speed = 22,
                HitRate = 80,
                DodgeRate = 18,
                CritRate = 18,
                CritDamage = 70,
                Faction = Faction.Enemy
            };
            enemies.Add(enemyMage);
            
            // 创建一个敌方弓箭手
            Entity enemyArcher = new Entity
            {
                Name = "精灵射手",
                MaxHealth = 110,
                Health = 110,
                Attack = 25,
                Defense = 12,
                Speed = 25,
                HitRate = 92,
                DodgeRate = 20,
                CritRate = 25,
                CritDamage = 60,
                Faction = Faction.Enemy
            };
            enemies.Add(enemyArcher);
            
            return enemies;
        }
        
        /// <summary>
        /// 显示战斗日志
        /// </summary>
        private static void DisplayBattleLog(List<string> log)
        {
            // 只显示当前回合的日志
            string lastRoundHeader = null;
            List<string> currentRoundLog = new List<string>();
            
            foreach (var logEntry in log)
            {
                if (logEntry.StartsWith("===== 回合"))
                {
                    lastRoundHeader = logEntry;
                    currentRoundLog.Clear();
                }
                else if (logEntry.StartsWith("===== 战斗结果"))
                {
                    // 战斗结果单独处理
                    break;
                }
                else if (!string.IsNullOrEmpty(lastRoundHeader))
                {
                    currentRoundLog.Add(logEntry);
                }
            }
            
            // 显示当前回合日志
            if (!string.IsNullOrEmpty(lastRoundHeader))
            {
                Console.WriteLine(lastRoundHeader);
                foreach (var logEntry in currentRoundLog)
                {
                    Console.WriteLine(logEntry);
                }
                Console.WriteLine();
            }
        }
        
        /// <summary>
        /// 显示当前状态
        /// </summary>
        private static void DisplayCurrentStatus(List<Entity> allies, List<Entity> enemies)
        {
            Console.WriteLine("当前状态：");
            
            Console.WriteLine("我方队伍：");
            foreach (var ally in allies)
            {
                string status = ally.IsAlive ? $"生命: {ally.Health}/{ally.MaxHealth}" : "已死亡";
                Console.WriteLine($"- {ally.Name}: {status}");
                
                // 显示状态效果
                if (ally.IsAlive && ally.StatusEffects.Count > 0)
                {
                    foreach (var effect in ally.StatusEffects)
                    {
                        Console.WriteLine($"  * {effect.Name} ({effect.RemainingDuration}回合)");
                    }
                }
            }
            
            Console.WriteLine("敌方队伍：");
            foreach (var enemy in enemies)
            {
                string status = enemy.IsAlive ? $"生命: {enemy.CurrentHealth}/{enemy.MaxHealth}" : "已死亡";
                Console.WriteLine($"- {enemy.Name}: {status}");
                
                // 显示状态效果
                if (enemy.IsAlive && enemy.StatusEffects.Count > 0)
                {
                    foreach (var effect in enemy.StatusEffects)
                    {
                        Console.WriteLine($"  * {effect.Name} ({effect.RemainingDuration}回合)");
                    }
                }
            }
            
            Console.WriteLine();
        }
        
        /// <summary>
        /// 显示战斗结果
        /// </summary>
        private static void DisplayBattleResult(BattleResult result)
        {
            Console.WriteLine("===== 战斗结果 =====");
            
            if (result.Winner == BattleWinner.Allies)
            {
                Console.WriteLine("恭喜！我方取得了胜利！");
            }
            else if (result.Winner == BattleWinner.Enemies)
            {
                Console.WriteLine("很遗憾，敌方取得了胜利！");
            }
            
            Console.WriteLine($"战斗共进行了 {result.RoundsFought} 回合。");
            
            Console.WriteLine("存活的我方角色：");
            if (result.RemainingAllies.Count > 0)
            {
                foreach (var ally in result.RemainingAllies)
                {
                    Console.WriteLine($"- {ally.Name} (剩余生命: {ally.CurrentHealth}/{ally.MaxHealth})");
                }
            }
            else
            {
                Console.WriteLine("无存活角色");
            }
            
            Console.WriteLine("存活的敌方角色：");
            if (result.RemainingEnemies.Count > 0)
            {
                foreach (var enemy in result.RemainingEnemies)
                {
                    Console.WriteLine($"- {enemy.Name} (剩余生命: {enemy.CurrentHealth}/{enemy.MaxHealth})");
                }
            }
            else
            {
                Console.WriteLine("无存活角色");
            }
            
            Console.WriteLine();
        }
    }
}