using System; using System.Collections.Generic; using System.Linq; using BattleSimulator.Core.Models;

namespace BattleSimulator.Core
{
    /// <summary>
    /// 战斗系统核心类，负责控制整个战斗流程
    /// </summary>
    public class BattleSystem
    {
        private List<Entity> _allies = new List<Entity>();
        private List<Entity> _enemies = new List<Entity>();
        private List<Entity> _allEntities = new List<Entity>();
        private int _currentRound = 0;
        private BattleResult _battleResult = null!;
        private Random _random = new Random();
        
        /// <summary>
        /// 获取当前回合数
        /// </summary>
        public int CurrentRound => _currentRound;
        
        /// <summary>
        /// 获取战斗结果
        /// </summary>
        public BattleResult BattleResult => _battleResult;
        
        /// <summary>
        /// 获取是否战斗结束
        /// </summary>
        public bool IsBattleOver => _battleResult != null;
        
        /// <summary>
        /// 初始化战斗
        /// </summary>
        /// <param name="allies">我方队伍</param>
        /// <param name="enemies">敌方队伍</param>
        public void InitializeBattle(List<Entity> allies, List<Entity> enemies)
        {
            _allies = allies ?? throw new ArgumentNullException(nameof(allies));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            
            // 合并所有实体
            _allEntities = new List<Entity>();
            _allEntities.AddRange(allies);
            _allEntities.AddRange(enemies);
            
            // 初始化每个实体的战斗数据
            foreach (var entity in _allEntities)
            {
                entity.InitializeForBattle();
            }
            
            // 重置回合数
            _currentRound = 0;
            _battleResult = null!;
        }
        
        /// <summary>
        /// 执行一整个回合的战斗
        /// </summary>
        public void ExecuteRound()
        {
            if (IsBattleOver)
            {
                throw new InvalidOperationException("战斗已经结束，无法继续执行回合");
            }
            
            // 增加回合数
            _currentRound++;
            
            // 更新所有实体的战斗日志
            foreach (var entity in _allEntities)
            {
                entity.BattleLog.Add($"===== 回合 {_currentRound} =====");
            }
            
            // 按照速度排序确定行动顺序
            var actionOrder = DetermineActionOrder();
            
            // 更新所有实体的状态效果
            UpdateStatusEffects();
            
            // 更新所有实体的技能冷却
            UpdateSkillCooldowns();
            
            // 检查战斗是否结束
            if (CheckBattleOver())
            {
                return;
            }
            
            // 按照行动顺序执行每个实体的行动
            foreach (var entity in actionOrder)
            {
                if (!entity.IsAlive)
                {
                    continue;
                }
                
                // 执行实体的行动
                ExecuteEntityAction(entity);
                
                // 检查战斗是否结束
                if (CheckBattleOver())
                {
                    return;
                }
            }
        }
        
        /// <summary>
        /// 执行实体的行动
        /// </summary>
        /// <param name="entity">要执行行动的实体</param>
        private void ExecuteEntityAction(Entity entity)
        {
            // 如果实体无法行动，则跳过
            if (!entity.CanAct)
            {
                entity.BattleLog.Add($"{entity.Name} 无法行动！");
                return;
            }
            
            // 选择目标（简单AI逻辑，实际项目中可以实现更复杂的AI）
            List<Entity> targets = ChooseTargets(entity);
            
            if (targets == null || targets.Count == 0)
            {
                entity.BattleLog.Add($"{entity.Name} 没有可攻击的目标！");
                return;
            }
            
            // 选择技能（简单AI逻辑，实际项目中可以实现更复杂的AI）
            Skill skill = ChooseSkill(entity);
            
            if (skill == null)
            {
                // 如果没有可用技能，使用普通攻击
                PerformBasicAttack(entity, targets[0]);
            }
            else
            {
                // 使用技能
                SkillResult result = skill.Use(entity, targets);
                
                // 更新战斗日志
                if (!string.IsNullOrEmpty(result.Message))
                {
                    foreach (var line in result.Message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        foreach (var e in _allEntities)
                        {
                            e.BattleLog.Add(line);
                        }
                    }
                }
                
                // 处理状态效果
                if (result.StatusEffectsApplied != null && result.StatusEffectsApplied.Count > 0)
                {
                    foreach (var effect in result.StatusEffectsApplied)
                    {
                        foreach (var target in targets)
                        {
                            ApplyStatusEffect(target, effect.Clone());
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 执行普通攻击
        /// </summary>
        private void PerformBasicAttack(Entity attacker, Entity target)
        {
            if (!target.IsAlive)
            {
                attacker.BattleLog.Add($"{attacker.Name} 尝试攻击已经死亡的目标！");
                return;
            }
            
            // 计算基础伤害
            int baseDamage = attacker.Attack - target.Defense / 2;
            baseDamage = Math.Max(1, baseDamage);
            
            // 检查是否命中
            bool isHit = CheckHit(attacker, target);
            
            if (isHit)
            {
                // 检查是否暴击
                bool isCritical = CheckCritical(attacker);
                
                // 计算最终伤害
                int finalDamage = isCritical ? (int)(baseDamage * (1 + attacker.CritDamage / 100.0)) : baseDamage;
                
                // 目标受到伤害
                int actualDamage = target.TakeDamage(finalDamage);
                
                // 更新战斗日志
                string logMessage = isCritical
                    ? $"{attacker.Name} 对 {target.Name} 造成了 {actualDamage} 点暴击伤害！"
                    : $"{attacker.Name} 对 {target.Name} 造成了 {actualDamage} 点伤害！";
                
                foreach (var entity in _allEntities)
                {
                    entity.BattleLog.Add(logMessage);
                }
                
                // 检查目标是否死亡
                if (!target.IsAlive)
                {
                    string deathMessage = $"{target.Name} 被击败了！";
                    foreach (var entity in _allEntities)
                    {
                        entity.BattleLog.Add(deathMessage);
                    }
                }
            }
            else
            {
                // 未命中
                string logMessage = $"{attacker.Name} 的攻击被 {target.Name} 闪避了！";
                foreach (var entity in _allEntities)
                {
                    entity.BattleLog.Add(logMessage);
                }
            }
        }
        
        /// <summary>
        /// 确定行动顺序
        /// </summary>
        private List<Entity> DetermineActionOrder()
        {
            return _allEntities
                .Where(e => e.IsAlive)
                .OrderByDescending(e => e.Speed + _random.Next(-5, 6)) // 速度+随机值，增加战斗的不确定性
                .ToList();
        }
        
        /// <summary>
        /// 更新所有实体的状态效果
        /// </summary>
        private void UpdateStatusEffects()
        {
            foreach (var entity in _allEntities.Where(e => e.IsAlive))
            {
                entity.UpdateStatusEffects();
            }
        }
        
        /// <summary>
        /// 更新所有实体的技能冷却
        /// </summary>
        private void UpdateSkillCooldowns()
        {
            foreach (var entity in _allEntities.Where(e => e.IsAlive))
            {
                foreach (var skill in entity.Skills)
                {
                    skill.UpdateCooldown();
                }
            }
        }
        
        /// <summary>
        /// 检查战斗是否结束
        /// </summary>
        private bool CheckBattleOver()
        {
            // 检查我方是否全灭
            bool alliesDefeated = !_allies.Any(e => e.IsAlive);
            
            // 检查敌方是否全灭
            bool enemiesDefeated = !_enemies.Any(e => e.IsAlive);
            
            if (alliesDefeated || enemiesDefeated)
            {
                // 战斗结束，生成战斗结果
                _battleResult = new BattleResult
                {
                    Winner = enemiesDefeated ? BattleWinner.Allies : BattleWinner.Enemies,
                    RoundsFought = _currentRound,
                    RemainingAllies = _allies.Where(e => e.IsAlive).ToList(),
                    RemainingEnemies = _enemies.Where(e => e.IsAlive).ToList()
                };
                
                // 更新战斗日志
                string resultMessage = enemiesDefeated
                    ? "战斗结束，我方胜利！"
                    : "战斗结束，敌方胜利！";
                
                foreach (var entity in _allEntities)
                {
                    entity.BattleLog.Add("===== 战斗结果 =====");
                    entity.BattleLog.Add(resultMessage);
                }
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 应用状态效果
        /// </summary>
        private void ApplyStatusEffect(Entity target, StatusEffect effect)
        {
            // 检查目标是否已经有相同的状态效果
            var existingEffect = target.StatusEffects.Find(e => e.Type == effect.Type);
            
            if (existingEffect != null)
            {
                // 如果效果可以堆叠，则增加堆叠层数
                if (existingEffect.Stack())
                {
                    // 增加堆叠层数后刷新持续时间
                    existingEffect.RemainingDuration = Math.Max(existingEffect.RemainingDuration, effect.Duration);
                }
                else
                {
                    // 如果不能堆叠或者已经达到最大堆叠层数，则刷新持续时间
                    existingEffect.RemainingDuration = Math.Max(existingEffect.RemainingDuration, effect.Duration);
                }
            }
            else
            {
                // 应用新的状态效果
                effect.Initialize();
                effect.Apply(target);
                target.StatusEffects.Add(effect);
            }
        }
        
        /// <summary>
        /// 检查是否命中
        /// </summary>
        private bool CheckHit(Entity attacker, Entity target)
        {
            // 命中率 = 攻击者命中率 - 目标闪避率
            int hitChance = attacker.HitRate - target.DodgeRate;
            // 确保命中率在5%到95%之间
            hitChance = Math.Max(5, Math.Min(95, hitChance));
            
            return _random.Next(100) < hitChance;
        }
        
        /// <summary>
        /// 检查是否暴击
        /// </summary>
        private bool CheckCritical(Entity attacker)
        {
            // 暴击率受攻击者暴击属性影响
            int critChance = attacker.CritRate;
            // 确保暴击率在1%到50%之间
            critChance = Math.Max(1, Math.Min(50, critChance));
            
            return _random.Next(100) < critChance;
        }
        
        /// <summary>
        /// 选择目标（简单AI逻辑）
        /// </summary>
        private List<Entity> ChooseTargets(Entity entity)
        {
            // 判断实体是我方还是敌方
            bool isAlly = _allies.Contains(entity);
            
            // 选择敌方作为目标
            List<Entity> potentialTargets = isAlly ? _enemies : _allies;
            
            // 过滤出活着的目标
            potentialTargets = potentialTargets.Where(e => e.IsAlive).ToList();
            
            if (potentialTargets.Count == 0)
            {
                return new List<Entity>();
            }
            
            // 简单AI：选择第一个目标
            return new List<Entity> { potentialTargets[0] };
        }
        
        /// <summary>
        /// 选择技能（简单AI逻辑）
        /// </summary>
        private Skill ChooseSkill(Entity entity)
        {
            if (entity.Skills == null || entity.Skills.Count == 0)
            {
                return null;
            }
            
            // 简单AI：选择第一个可用的技能
            return entity.Skills.FirstOrDefault(s => s.CanUse(entity));
        }
    }
    
    /// <summary>
    /// 战斗结果
    /// </summary>
    public class BattleResult
    {
        public BattleWinner Winner { get; set; } = BattleWinner.None;
        public int RoundsFought { get; set; } = 0;
        public List<Entity> RemainingAllies { get; set; } = new List<Entity>();
        public List<Entity> RemainingEnemies { get; set; } = new List<Entity>();
    }
    
    /// <summary>
    /// 战斗胜利者
    /// </summary>
    public enum BattleWinner
    {
        None,
        Allies,
        Enemies
    }
}