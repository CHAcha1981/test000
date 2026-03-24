using System.Collections.Generic;

namespace BattleSimulator.Core.Models
{
    /// <summary>
    /// 战斗实体基类，定义了参与战斗的所有角色或单位的基本属性和行为
    /// </summary>
    public class Entity
    {
        // 基本属性
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; set; } = 100;
        public int Attack { get; set; } = 10;
        public int Defense { get; set; } = 5;
        public int Speed { get; set; } = 5;
        public int CritRate { get; set; } = 5; // 暴击率（百分比）
        public int CritDamage { get; set; } = 50; // 暴击伤害加成（百分比）
        public int HitRate { get; set; } = 90; // 命中率（百分比）
        public int DodgeRate { get; set; } = 10; // 闪避率（百分比）
        
        // 状态效果集合
        public List<StatusEffect> StatusEffects { get; } = new List<StatusEffect>();
        
        // 技能列表
        public List<Skill> Skills { get; } = new List<Skill>();
        
        // 阵营标识
        public Faction Faction { get; set; } = Faction.Neutral;
        
        // 是否存活
        public bool IsAlive => CurrentHealth > 0;
        
        // 是否可以行动
        public bool CanAct { get; set; } = true;
        
        // 是否无敌
        public bool IsInvulnerable { get; set; } = false;
        
        // 战斗日志
        public List<string> BattleLog { get; } = new List<string>();
        
        /// <summary>
        /// 初始化实体状态
        /// </summary>
        public virtual void Initialize()
        {
            CurrentHealth = MaxHealth;
            StatusEffects.Clear();
            CanAct = true;
            IsInvulnerable = false;
            BattleLog.Clear();
        }
        
        /// <summary>
        /// 为战斗初始化实体
        /// </summary>
        public virtual void InitializeForBattle()
        {
            Initialize();
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <returns>实际受到的伤害</returns>
        public virtual int TakeDamage(int damage)
        {
            // 如果实体处于无敌状态，不受任何伤害
            if (IsInvulnerable)
            {
                return 0;
            }
            
            // 检查是否有减伤状态效果
            int finalDamage = ApplyDamageModifiers(damage);
            
            // 确保生命值不会低于0
            CurrentHealth = System.Math.Max(0, CurrentHealth - finalDamage);
            
            return finalDamage;
        }
        
        /// <summary>
        /// 恢复生命值
        /// </summary>
        /// <param name="healAmount">治疗量</param>
        /// <returns>实际恢复的生命值</returns>
        public virtual int Heal(int healAmount)
        {
            // 检查是否有治疗加成状态效果
            int finalHeal = ApplyHealModifiers(healAmount);
            
            // 确保生命值不会超过最大值
            int actualHeal = finalHeal;
            if (CurrentHealth + finalHeal > MaxHealth)
            {
                actualHeal = MaxHealth - CurrentHealth;
            }
            
            CurrentHealth += actualHeal;
            
            return actualHeal;
        }
        
        /// <summary>
        /// 添加状态效果
        /// </summary>
        /// <param name="effect">要添加的状态效果</param>
        public virtual void AddStatusEffect(StatusEffect effect)
        {
            // 检查是否已经存在相同类型的效果
            var existingEffect = StatusEffects.Find(e => e.Type == effect.Type);
            
            if (existingEffect != null)
            {
                // 如果存在，叠加效果或刷新持续时间
                existingEffect.StackCount = System.Math.Min(existingEffect.MaxStacks, existingEffect.StackCount + effect.StackCount);
                existingEffect.RemainingDuration = System.Math.Max(existingEffect.RemainingDuration, effect.RemainingDuration);
            }
            else
            {
                StatusEffects.Add(effect);
            }
        }
        
        /// <summary>
        /// 移除状态效果
        /// </summary>
        /// <param name="effectType">要移除的状态效果类型</param>
        public virtual void RemoveStatusEffect(StatusEffectType effectType)
        {
            var effect = StatusEffects.Find(e => e.Type == effectType);
            if (effect != null)
            {
                StatusEffects.Remove(effect);
            }
        }
        
        /// <summary>
        /// 应用伤害修饰符
        /// </summary>
        /// <param name="damage">原始伤害</param>
        /// <returns>修饰后的伤害</returns>
        protected virtual int ApplyDamageModifiers(int damage)
        {
            int modifiedDamage = damage;
            
            // 应用所有状态效果对伤害的影响
            foreach (var effect in StatusEffects)
            {
                modifiedDamage = effect.ModifyIncomingDamage(modifiedDamage);
            }
            
            // 确保伤害不会小于1
            return System.Math.Max(1, modifiedDamage);
        }
        
        /// <summary>
        /// 应用治疗修饰符
        /// </summary>
        /// <param name="healAmount">原始治疗量</param>
        /// <returns>修饰后的治疗量</returns>
        protected virtual int ApplyHealModifiers(int healAmount)
        {
            int modifiedHeal = healAmount;
            
            // 应用所有状态效果对治疗的影响
            foreach (var effect in StatusEffects)
            {
                modifiedHeal = effect.ModifyIncomingHeal(modifiedHeal);
            }
            
            return modifiedHeal;
        }
        
        /// <summary>
        /// 更新状态效果持续时间
        /// </summary>
        public virtual void UpdateStatusEffects()
        {
            for (int i = StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = StatusEffects[i];
                effect.RemainingDuration--;
                
                // 应用状态效果的回合效果
                effect.OnTurnTick(this);
                
                // 如果持续时间结束，移除效果
                if (effect.RemainingDuration <= 0)
                {
                    effect.OnEffectEnd(this);
                    StatusEffects.RemoveAt(i);
                }
            }
        }
    }
    
    /// <summary>
    /// 阵营枚举
    /// </summary>
    public enum Faction
    {
        Neutral,
        Player,
        Enemy,
        Ally
    }
}