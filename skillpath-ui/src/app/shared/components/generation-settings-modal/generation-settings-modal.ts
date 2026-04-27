import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalComponent } from '../modal/modal';
import { ButtonComponent } from '../button/button';
import { BadgeComponent } from '../badge/badge';

export interface GenerationSettings {
  minSkills: number;
  maxSkills: number;
  tasksPerSkill: number;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  focus: 'Breadth' | 'Depth' | 'Balanced';
  additionalContext?: string;
}

@Component({
  selector: 'app-generation-settings-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, ModalComponent, ButtonComponent, BadgeComponent],
  templateUrl: './generation-settings-modal.html',
  styleUrls: ['./generation-settings-modal.scss']
})
export class GenerationSettingsModal {
  @Input() isOpen = false;
  @Output() closed = new EventEmitter<void>();
  @Output() generate = new EventEmitter<GenerationSettings>();

  settings = signal<GenerationSettings>({
    minSkills: 5,
    maxSkills: 12,
    tasksPerSkill: 5,
    difficulty: 'Intermediate',
    focus: 'Balanced'
  });

  minSkills = 5;
  additionalContext = '';

  setDifficulty(difficulty: 'Beginner' | 'Intermediate' | 'Advanced') {
    this.settings.update(s => ({ ...s, difficulty }));
  }

  setFocus(focus: 'Breadth' | 'Depth' | 'Balanced') {
    this.settings.update(s => ({ ...s, focus }));
  }

  setTaskCount(count: number) {
    this.settings.update(s => ({ ...s, tasksPerSkill: count }));
  }

  updateSkillRange() {
    const max = Math.max(this.minSkills + 3, this.minSkills + 7);
    this.settings.update(s => ({ 
      ...s, 
      minSkills: this.minSkills, 
      maxSkills: max 
    }));
  }

  resetToDefaults() {
    this.settings.set({
      minSkills: 5,
      maxSkills: 12,
      tasksPerSkill: 5,
      difficulty: 'Intermediate',
      focus: 'Balanced'
    });
    this.minSkills = 5;
    this.additionalContext = '';
  }

  getPreviewDescription(): string {
    const { difficulty, focus, minSkills, maxSkills, tasksPerSkill } = this.settings();
    
    return `Will generate a ${difficulty.toLowerCase()} level skill tree with ${focus.toLowerCase()} focus, containing ${minSkills}-${maxSkills} skills with ${tasksPerSkill} tasks each. ${
      difficulty === 'Beginner' ? 'Tasks will be simple and foundational.' :
      difficulty === 'Advanced' ? 'Tasks will be challenging and project-based.' :
      'Tasks will balance theory and practice.'
    }`;
  }

  onCancel() {
    this.closed.emit();
  }

  onGenerate() {
    const finalSettings = {
      ...this.settings(),
      additionalContext: this.additionalContext.trim() || undefined
    };
    this.generate.emit(finalSettings);
  }
}