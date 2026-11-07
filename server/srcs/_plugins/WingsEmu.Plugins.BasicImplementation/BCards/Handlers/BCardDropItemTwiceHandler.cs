// WingsEmu
// 
// Developed by NosWings Team

using System;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDropItemTwiceHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICardsManager _cardsManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;

    public BCardDropItemTwiceHandler(IRandomGenerator randomGenerator, ICardsManager cardsManager, IBuffFactory buffFactory, IGameLanguageService gameLanguage)
    {
        _randomGenerator = randomGenerator;
        _cardsManager = cardsManager;
        _buffFactory = buffFactory;
        _gameLanguage = gameLanguage;
    }

    public BCardType HandledType => BCardType.DropItemTwice;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileAttackingChance:
                {
                    if (sender.IsSameEntity(target))
                    {
                        return;
                    }

                    Buff b = _buffFactory.CreateBuff(secondData, sender);
                    if (b == null)
                    {
                        return;
                    }

                    double debuffCounter = target.CheckForResistance(b, _cardsManager, out double buffCounter, out double specializedResistance);

                    int randomNumber = _randomGenerator.RandomNumber();
                    int debuffRandomNumber = _randomGenerator.RandomNumber();
                    int buffRandomNumber = _randomGenerator.RandomNumber();
                    int specializedRandomNumber = _randomGenerator.RandomNumber();
                    switch (target)
                    {
                        case IMonsterEntity monster:
                            if (!monster.CanBeDebuffed)
                            {
                                return;
                            }

                            break;
                    }

                    if (b.CardId == (short)BuffVnums.MEMORIAL && sender.BuffComponent.HasBuff((short)BuffVnums.MEMORIAL))
                    {
                        return;
                    }

                    if (randomNumber > firstData)
                    {
                        return;
                    }

                    if (specializedRandomNumber >= (int)(specializedResistance * 100))
                    {
                        if (target is not IPlayerEntity c)
                        {
                            return;
                        }

                        string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                        c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                        return;
                    }

                    switch (b.BuffGroup)
                    {
                        case BuffGroup.Bad when debuffRandomNumber >= (int)(debuffCounter * 100):
                            {
                                if (target is not IPlayerEntity c)
                                {
                                    return;
                                }

                                string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                                c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                                return;
                            }
                        case BuffGroup.Good when buffRandomNumber >= (int)(buffCounter * 100):
                            {
                                if (target is not IPlayerEntity c)
                                {
                                    return;
                                }

                                string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                                c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                                return;
                            }
                    }
                    
                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                    break;
                }

            case (byte)AdditionalTypes.DropItemTwice.EffectOnSelfWhileAttackingChance:
                {
                    Buff b = _buffFactory.CreateBuff(secondData, sender);
                    if (b == null)
                    {
                        return;
                    }

                    double debuffCounter = target.CheckForResistance(b, _cardsManager, out double buffCounter, out double specializedResistance);

                    int randomNumber = _randomGenerator.RandomNumber();
                    int debuffRandomNumber = _randomGenerator.RandomNumber();
                    int buffRandomNumber = _randomGenerator.RandomNumber();
                    int specializedRandomNumber = _randomGenerator.RandomNumber();
                    if (b.CardId == (short)BuffVnums.MEMORIAL && sender.BuffComponent.HasBuff((short)BuffVnums.MEMORIAL))
                    {
                        return;
                    }

                    if (randomNumber > firstData)
                    {
                        return;
                    }

                    if (specializedRandomNumber >= (int)(specializedResistance * 100))
                    {
                        if (target is not IPlayerEntity c)
                        {
                            return;
                        }

                        string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                        c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                        return;
                    }

                    if (ctx.Skill?.Vnum is (short)SkillsVnums.FIRE_MINE or (short)SkillsVnums.BOMB)
                    {
                        if (sender.IsSameEntity(target))
                        {
                            return;
                        }
                    }

                    switch (b.BuffGroup)
                    {
                        case BuffGroup.Bad when debuffRandomNumber >= (int)(debuffCounter * 100):
                            {
                                if (target is not IPlayerEntity c)
                                {
                                    return;
                                }

                                string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                                c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                                return;
                            }
                        case BuffGroup.Good when buffRandomNumber >= (int)(buffCounter * 100):
                            {
                                if (target is not IPlayerEntity c)
                                {
                                    return;
                                }

                                string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                                c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                                return;
                            }
                        case BuffGroup.Bad when target is IMonsterEntity monsterEntity:
                            monsterEntity.MapInstance.AddEntityToTargets(monsterEntity, sender);
                            break;
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    int firstRandomNumber = _randomGenerator.RandomNumber();
                    int secondRandomNumber = _randomGenerator.RandomNumber();
                    if (target.BCardComponent.HasBCard(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectBadEffect) && firstRandomNumber <= secondRandomNumber &&
                        buff.BuffGroup == BuffGroup.Bad)
                    {
                        sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                        return;
                    }

                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                break;

            case (byte)AdditionalTypes.DropItemTwice.EffectOnEnemyWhileDefendingChance:
                {
                    if (sender.IsSameEntity(target))
                    {
                        return;
                    }

                    Buff b = _buffFactory.CreateBuff(secondData, sender);
                    if (b == null)
                    {
                        return;
                    }

                    double debuffCounter = target.CheckForResistance(b, _cardsManager, out double buffCounter, out double specializedResistance);

                    int randomNumber = _randomGenerator.RandomNumber();
                    int debuffRandomNumber = _randomGenerator.RandomNumber();
                    int buffRandomNumber = _randomGenerator.RandomNumber();
                    int specializedRandomNumber = _randomGenerator.RandomNumber();
                    switch (target)
                    {
                        case IMonsterEntity monster:
                            if (!monster.CanBeDebuffed)
                            {
                                return;
                            }

                            break;
                    }

                    if (b.CardId == (short)BuffVnums.MEMORIAL && sender.BuffComponent.HasBuff((short)BuffVnums.MEMORIAL))
                    {
                        return;
                    }

                    if (randomNumber > firstData)
                    {
                        return;
                    }

                    if (specializedRandomNumber >= (int)(specializedResistance * 100))
                    {
                        if (target is not IPlayerEntity c)
                        {
                            return;
                        }

                        string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                        c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                        return;
                    }

                    switch (b.BuffGroup)
                    {
                        case BuffGroup.Bad when debuffRandomNumber >= (int)(debuffCounter * 100):
                            {
                                if (target is not IPlayerEntity c)
                                {
                                    return;
                                }

                                string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                                c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                                return;
                            }
                        case BuffGroup.Good when buffRandomNumber >= (int)(buffCounter * 100):
                            {
                                if (target is not IPlayerEntity c)
                                {
                                    return;
                                }

                                string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                                c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                                return;
                            }
                    }

                    Buff buff = _buffFactory.CreateBuff(secondData, sender);
                    target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                    break;
                }
        }
    }
}