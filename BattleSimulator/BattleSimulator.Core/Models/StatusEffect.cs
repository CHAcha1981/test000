using System;
using System.Collections.Generic;

namespace BattleSimulator.Core.Models
{
    /// <summary>
    /// 状态效果类型枚举
    /// </summary>
    public enum StatusEffectType
    {
        None,
        Poison,
        Stun,
        AttackBoost,
        Invulnerability,
        Regeneration,
        Burn,
        SpellPowerBoost
    }

    /// <summary>
    /// 状态效果基类，定义了各种可以施加在实体上的状态
    /// </summary>
    public abstract class StatusEffect
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Duration { get; set; } = 0;
        public int RemainingDuration { get; set; } = 0;
        public StatusEffectType Type { get; set; } = StatusEffectType.None;
        public bool IsDebuff { get; set; } = false;
        public bool IsStackable { get; set; } = false;
        public int MaxStacks { get; set; } = 1;
        public int StackCount { get; set; } = 1;
        
        /// <summary>
        /// 初始化状态效果
        /// </summary>
        public virtual void Initialize()
        {
            RemainingDuration = Duration;
            StackCount = 1;
        }
        
        /// <summary>
        /// 应用状态效果
        /// </summary>
        /// <param name="target">目标实体</param>
        public abstract void Apply(Entity target);
        
        /// <summary>
        /// 回合结束时的效果
        /// </summary>
        /// <param name="target">目标实体</param>
        public virtual void OnTurnTick(Entity target)
        {
            // 子类可以重写此方法以实现每回合的效果
        }
        
        /// <summary>
        /// 当状态效果结束时调用
        /// </summary>
        /// <param name="target">目标实体</param>
        public virtual void OnEffectEnd(Entity target)
        {
            // 子类可以重写此方法以实现状态效果结束时的行为
        }
        
        /// <summary>
        /// 增加状态效果的堆叠层数
        /// </summary>
        /// <returns>是否成功增加堆叠</returns>
        public virtual bool Stack()
        {
            if (!IsStackable || StackCount >= MaxStacks)
            {
                return false;
            }
            
            StackCount++;
            return true;
        }
        
        /// <summary>
        /// 修改传入的伤害
        /// </summary>
        /// <param name="damage">原始伤害</param>
        /// <returns>修改后的伤害</returns>
        public virtual int ModifyIncomingDamage(int damage)
        {
            return damage;
        }
        
        /// <summary>
        /// 修改传入的治疗量
        /// </summary>
        /// <param name="healAmount">原始治疗量</param>
        /// <returns>修改后的治疗量</returns>
        public virtual int ModifyIncomingHeal(int healAmount)
        {
            return healAmount;
        }
        
        /// <summary>
        /// 克隆状态效果
        /// </summary>
        /// <returns>状态效果的副本</returns>
        public abstract StatusEffect Clone();
    }
    
    /// <summary>
    /// 中毒状态
    /// </summary>
    public class PoisonEffect : StatusEffect
    {
        public int DamagePerTurn { get; set; } = 5;
        
        public PoisonEffect()
        {
            Id = "poison";
            Name = "中毒";
            Description = "每回合受到持续伤害";
            Duration = 3;
            Type = StatusEffectType.Poison;
            IsDebuff = true;
            IsStackable = true;
            MaxStacks = 5;
        }
        
        public override void Apply(Entity target)
        {
            // 首次应用时的效果
        }
        
        public override void OnTurnTick(Entity target)
        {
            // 每回合造成伤害
            int totalDamage = DamagePerTurn * StackCount;
            target.TakeDamage(totalDamage);
        }
        
        public override StatusEffect Clone()
        {
            return new PoisonEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                Type = this.Type,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                MaxStacks = this.MaxStacks,
                StackCount = this.StackCount,
                DamagePerTurn = this.DamagePerTurn
            };
        }
    }
    
    /// <summary>
    /// 眩晕状态
    /// </summary>
    public class StunEffect : StatusEffect
    {
        public StunEffect()
        {
            Id = "stun";
            Name = "眩晕";
            Description = "无法行动";
            Duration = 1;
            Type = StatusEffectType.Stun;
            IsDebuff = true;
            IsStackable = false;
        }
        
        public override void Apply(Entity target)
        {
            // 施加眩晕效果，使目标无法行动
            target.CanAct = false;
        }
        
        public override void OnEffectEnd(Entity target)
        {
            // 眩晕效果结束，恢复目标行动能力
            target.CanAct = true;
        }
        
        public override StatusEffect Clone()
        {
            return new StunEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                Type = this.Type,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                StackCount = this.StackCount
            };
        }
    }
    
    /// <summary>
    /// 攻击力提升状态
    /// </summary>
    public class AttackBoostEffect : StatusEffect
    {
        public int AttackIncrease { get; set; } = 10;
        
        public AttackBoostEffect()
        {
            Id = "attack_boost";
            Name = "攻击力提升";
            Description = "攻击力临时增加";
            Duration = 3;
            Type = StatusEffectType.AttackBoost;
            IsDebuff = false;
            IsStackable = false;
        }
        
        public override void Apply(Entity target)
        {
            // 增加目标攻击力
            target.Attack += AttackIncrease;
        }
        
        public override void OnEffectEnd(Entity target)
        {
            // 恢复目标攻击力
            target.Attack -= AttackIncrease;
        }
        
        public override StatusEffect Clone()
        {
            return new AttackBoostEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                Type = this.Type,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                StackCount = this.StackCount,
                AttackIncrease = this.AttackIncrease
            };
        }
    }
    
    /// <summary>
    /// 无敌状态
    /// </summary>
    public class InvulnerabilityEffect : StatusEffect
    {
        public InvulnerabilityEffect()
        {
            Id = "invulnerability";
            Name = "无敌";
            Description = "免疫所有伤害";
            Duration = 1;
            Type = StatusEffectType.Invulnerability;
            IsDebuff = false;
            IsStackable = false;
        }
        
        public override void Apply(Entity target)
        {
            // 设置目标无敌
            target.IsInvulnerable = true;
        }
        
        public override void OnEffectEnd(Entity target)
        {
            // 取消目标无敌状态
            target.IsInvulnerable = false;
        }
        
        public override int ModifyIncomingDamage(int damage)
        {
            // 无敌状态下免疫所有伤害
            return 0;
        }
        
        public override StatusEffect Clone()
        {
            return new InvulnerabilityEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                Type = this.Type,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                StackCount = this.StackCount
            };
        }
    }
    
    /// <summary>
    /// 持续治疗状态
    /// </summary>
    public class RegenerationEffect : StatusEffect
    {
        public int HealPerTurn { get; set; } = 8;
        
        public RegenerationEffect()
        {
            Id = "regeneration";
            Name = "生命恢复";
            Description = "每回合恢复生命值";
            Duration = 4;
            Type = StatusEffectType.Regeneration;
            IsDebuff = false;
            IsStackable = true;
            MaxStacks = 3;
        }
        
        public override void Apply(Entity target)
        {
            // 首次应用时的效果
        }
        
        public override void OnTurnTick(Entity target)
        {
            // 每回合恢复生命值
            int totalHeal = HealPerTurn * StackCount;
            target.Heal(totalHeal);
        }
        
        public override StatusEffect Clone()
        {
            return new RegenerationEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                Type = this.Type,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                MaxStacks = this.MaxStacks,
                StackCount = this.StackCount,
                HealPerTurn = this.HealPerTurn
            };
        }
    }
    
    /// <summary>
    /// 燃烧状态（在Mage.cs中被引用）
    /// </summary>
    public class BurnEffect : StatusEffect
    {
        public int DamagePerTurn { get; set; } = 8;
        
        public BurnEffect()
        {
            Id = "burn";
            Name = "燃烧";
            Description = "每回合受到火焰伤害";
            Duration = 2;
            Type = StatusEffectType.Burn;
            IsDebuff = true;
            IsStackable = false;
        }
        
        public override void Apply(Entity target)
        {
            // 首次应用时的效果
        }
        
        public override void OnTurnTick(Entity target)
        {
            // 每回合造成火焰伤害
            int totalDamage = DamagePerTurn * StackCount;
            target.TakeDamage(totalDamage);
        }
        
        public override StatusEffect Clone()
        {
            return new BurnEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                Type = this.Type,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                StackCount = this.StackCount,
                DamagePerTurn = this.DamagePerTurn
            };
        }
    }
    
    /// <summary>
    /// 魔法强度提升状态（在Mage.cs中被引用）
    /// </summary>
    public class SpellPowerBoostEffect : StatusEffect
    {
        public int SpellPowerIncrease { get; set; } = 15;
        
        public SpellPowerBoostEffect()
        {
            Id = "spell_power_boost";
            Name = "魔法强化";
            Description = "魔法强度提升";
            Duration = 3;
            Type = StatusEffectType.SpellPowerBoost;
            IsDebuff = false;
            IsStackable = true;
            MaxStacks = 2;
        }
        
        public override void Apply(Entity target)
        {
            // 增加目标攻击力（代表法术强度）
            target.Attack += SpellPowerIncrease;
        }
        
        public override void OnEffectEnd(Entity target)
        {
            // 恢复目标攻击力
            target.Attack -= SpellPowerIncrease * StackCount;
        }
        
        public override StatusEffect Clone()
        {
            return new SpellPowerBoostEffect
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Duration = this.Duration,
                Type = this.Type,
                IsDebuff = this.IsDebuff,
                IsStackable = this.IsStackable,
                MaxStacks = this.MaxStacks,
                StackCount = this.StackCount,
                SpellPowerIncrease = this.SpellPowerIncrease
            };
        }
    }
}