using System.Collections.Generic;
using System.Linq;
using Articy.Unity.Utils;
using DarkTonic.MasterAudio;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Playables;

namespace SpecialAssignment
{
    public class DialogueMediaPlayer
    {
        private DirectorReferences directors;
        
        private MediaType lastPlayedType;
        private PlayableDirector lastTimeline;
        private PlaySoundResult lastSound;
        private string lastSpeaker;
        
        private static Dictionary<string, CharacterReferences> characterReferences = new();
        
        private const string TEMP_TIMELINE_TRIGGER_LINE = "002_B";

        public DialogueMediaPlayer(DirectorReferences directors)
        {
            this.directors = directors;
            lastPlayedType = MediaType.None;
        }

        public static void AddCharacterReferences(CharacterReferences characterReference)
        {
            characterReferences.Add(characterReference.characterName, characterReference);
        }

        public void Play(string assetId, string speaker, string line)
        {
            StopPreviousMedia();

            // TODO: Switch this check out for checking for "Timeline" camera type when it exists
            if (line.Equals(TEMP_TIMELINE_TRIGGER_LINE))
            {
                var director = directors.GetDirector(assetId);
                if (director != null)
                {
                    director.Play();
                    lastPlayedType = MediaType.Timeline;
                    lastTimeline = director;
                }
                else
                    Debug.LogWarning($"[Dialogue] Couldn't find playable director for assetId: {assetId}");

                return;
            }

            if (speaker.IsNullOrWhiteSpace() || !characterReferences.ContainsKey(speaker))
            {
                Debug.LogWarning($"[Dialogue] Speaker {speaker} is not in characterReferences");
                return;
            }

            lastPlayedType = MediaType.Audio;
            lastSound = MasterAudio.PlaySound3DAtTransform(assetId, characterReferences[speaker].audioTransform);
            
            // Handle animator change
            Animator speakerAnimator = characterReferences[speaker].animator;
            AnimatorController animatorController = speakerAnimator.runtimeAnimatorController as AnimatorController;
            AnimatorControllerLayer mainLayer = animatorController?.layers[0];
            
            bool lastSpeakerExists = !lastSpeaker.IsNullOrWhiteSpace() && characterReferences.ContainsKey(lastSpeaker);
            bool hasAnimation = mainLayer?.stateMachine.states.Any(x => assetId.Equals(x.state.name)) ?? false;
            bool speakersAreDifferent = !speaker.Equals(lastSpeaker);

            if (lastSpeakerExists && speakersAreDifferent)
            {
                var lastAnimator = characterReferences[lastSpeaker].animator;
                var lastIdle = characterReferences[lastSpeaker].idleName;
                lastAnimator.CrossFade(lastIdle, 0.2f);
            }

            if (lastSpeakerExists && !hasAnimation)
            {
                var idle =  characterReferences[speaker].idleName;
                speakerAnimator.CrossFade(idle, 0.2f);
            }
            else if (hasAnimation && (lastSpeakerExists || speakersAreDifferent))
            {
                speakerAnimator.CrossFade(assetId, 0.2f);
            }
            
            lastSpeaker = speaker;
        }

        private void StopPreviousMedia()
        {
            if (lastPlayedType == MediaType.None)
                return;

            if (lastPlayedType == MediaType.Timeline)
            {
                if (lastTimeline != null && lastTimeline.state == PlayState.Playing)
                {
                    // Skip to end of timeline so everything is in position, and stop it
                    lastTimeline.time = lastTimeline.duration;
                    lastTimeline.Stop();
                }
                
                return;
            }
            
            if (lastSound != null && lastSound.ActingVariation.IsPlaying)
                lastSound.ActingVariation.Stop();
        }

        private enum MediaType
        {
            None,
            Audio,
            Timeline,
        }
    }
}