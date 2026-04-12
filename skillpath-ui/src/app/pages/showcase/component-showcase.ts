import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../../shared/components/button/button';
import { CardComponent } from '../../shared/components/card/card';
import { BadgeComponent } from '../../shared/components/badge/badge';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar';
import { ModalComponent } from '../../shared/components/modal/modal';
import { SpinnerComponent } from '../../shared/components/spinner/spinner';

@Component({
  selector: 'app-component-showcase',
  standalone: true,
  imports: [
    CommonModule,
    ButtonComponent,
    CardComponent,
    BadgeComponent,
    ProgressBarComponent,
    ModalComponent,
    SpinnerComponent
  ],
  template: `
    <div class="showcase">
      <header class="showcase-header">
        <h1>Component Library Showcase</h1>
        <p>Test and preview all shared components</p>
      </header>

      <!-- Buttons Section -->
      <section class="showcase-section">
        <h2>Buttons</h2>
        
        <div class="showcase-group">
          <h3>Variants</h3>
          <div class="showcase-row">
            <app-button variant="primary">Primary</app-button>
            <app-button variant="secondary">Secondary</app-button>
            <app-button variant="danger">Danger</app-button>
            <app-button variant="ghost">Ghost</app-button>
          </div>
        </div>

        <div class="showcase-group">
          <h3>Sizes</h3>
          <div class="showcase-row">
            <app-button size="sm">Small</app-button>
            <app-button size="md">Medium</app-button>
            <app-button size="lg">Large</app-button>
          </div>
        </div>

        <div class="showcase-group">
          <h3>States</h3>
          <div class="showcase-row">
            <app-button [disabled]="true">Disabled</app-button>
            <app-button [loading]="true">Loading</app-button>
            <app-button [fullWidth]="true">Full Width</app-button>
          </div>
        </div>
      </section>

      <!-- Cards Section -->
      <section class="showcase-section">
        <h2>Cards</h2>
        
        <div class="showcase-grid">
          <app-card variant="default" [clickable]="true" [hasHeader]="true" [hasFooter]="true">
            <div card-header>
              <h3>Default Card</h3>
              <app-badge variant="active">Active</app-badge>
            </div>
            <p>This is a default card with header and footer. Click me!</p>
            <div card-footer>
              <app-button size="sm" variant="ghost">Cancel</app-button>
              <app-button size="sm">Action</app-button>
            </div>
          </app-card>

          <app-card variant="outlined" [clickable]="true">
            <h3>Outlined Card</h3>
            <p>This card has an outlined variant with thicker border.</p>
          </app-card>

          <app-card variant="elevated">
            <h3>Elevated Card</h3>
            <p>This card has a shadow elevation effect.</p>
          </app-card>
        </div>
      </section>

      <!-- Badges Section -->
      <section class="showcase-section">
        <h2>Badges</h2>
        
        <div class="showcase-group">
          <h3>Goal Status</h3>
          <div class="showcase-row">
            <app-badge variant="draft">Draft</app-badge>
            <app-badge variant="active">Active</app-badge>
            <app-badge variant="completed">Completed</app-badge>
            <app-badge variant="archived">Archived</app-badge>
          </div>
        </div>

        <div class="showcase-group">
          <h3>Skill Status</h3>
          <div class="showcase-row">
            <app-badge variant="locked">Locked</app-badge>
            <app-badge variant="available">Available</app-badge>
            <app-badge variant="in-progress">In Progress</app-badge>
            <app-badge variant="completed">Completed</app-badge>
          </div>
        </div>

        <div class="showcase-group">
          <h3>With Dot Indicator</h3>
          <div class="showcase-row">
            <app-badge variant="success" [showDot]="true">Success</app-badge>
            <app-badge variant="warning" [showDot]="true">Warning</app-badge>
            <app-badge variant="error" [showDot]="true">Error</app-badge>
          </div>
        </div>

        <div class="showcase-group">
          <h3>Sizes</h3>
          <div class="showcase-row">
            <app-badge size="sm" variant="info">Small</app-badge>
            <app-badge size="md" variant="info">Medium</app-badge>
            <app-badge size="lg" variant="info">Large</app-badge>
          </div>
        </div>
      </section>

      <!-- Progress Bars Section -->
      <section class="showcase-section">
        <h2>Progress Bars</h2>
        
        <div class="showcase-group">
          <h3>Variants</h3>
          <app-progress-bar
            label="Default Progress"
            [current]="65"
            [max]="100"
            variant="default"
          />
          <app-progress-bar
            label="XP Progress"
            [current]="850"
            [max]="1000"
            unit="XP"
            variant="xp"
          />
          <app-progress-bar
            label="Success"
            [current]="100"
            [max]="100"
            variant="success"
          />
        </div>

        <div class="showcase-group">
          <h3>Sizes</h3>
          <app-progress-bar
            label="Small"
            [current]="40"
            [max]="100"
            size="sm"
          />
          <app-progress-bar
            label="Medium"
            [current]="60"
            [max]="100"
            size="md"
          />
          <app-progress-bar
            label="Large"
            [current]="80"
            [max]="100"
            size="lg"
          />
        </div>

        <div class="showcase-group">
          <h3>With Percentage</h3>
          <app-progress-bar
            label="Completion"
            [current]="750"
            [max]="1000"
            [showPercentage]="true"
            variant="xp"
          />
        </div>
      </section>

      <!-- Spinners Section -->
      <section class="showcase-section">
        <h2>Spinners</h2>
        
        <div class="showcase-group">
          <h3>Sizes</h3>
          <div class="showcase-row">
            <app-spinner size="xs" />
            <app-spinner size="sm" />
            <app-spinner size="md" />
            <app-spinner size="lg" />
            <app-spinner size="xl" />
          </div>
        </div>

        <div class="showcase-group">
          <h3>Variants</h3>
          <div class="showcase-row">
            <app-spinner variant="default" />
            <app-spinner variant="primary" />
            <app-spinner variant="success" />
            <app-spinner variant="warning" />
            <app-spinner variant="danger" />
          </div>
        </div>

        <div class="showcase-group">
          <h3>With Text</h3>
          <app-spinner size="lg" text="Loading your skill tree..." />
        </div>

        <div class="showcase-group">
          <h3>Inline</h3>
          <div class="showcase-row">
            <app-spinner size="sm" [inline]="true" />
            <span style="color: var(--text-secondary)">Loading inline...</span>
          </div>
        </div>
      </section>

      <!-- Modal Section -->
      <section class="showcase-section">
        <h2>Modals</h2>
        
        <div class="showcase-row">
          <app-button (clicked)="showModalSm.set(true)">Small Modal</app-button>
          <app-button (clicked)="showModalMd.set(true)">Medium Modal</app-button>
          <app-button (clicked)="showModalLg.set(true)">Large Modal</app-button>
        </div>

        <!-- Small Modal -->
        <app-modal
          [isOpen]="showModalSm()"
          size="sm"
          (closed)="showModalSm.set(false)"
        >
          <h2 modal-header>Small Modal</h2>
          <p>This is a small modal dialog with a concise message.</p>
          <div modal-footer>
            <app-button variant="secondary" (clicked)="showModalSm.set(false)">
              Close
            </app-button>
          </div>
        </app-modal>

        <!-- Medium Modal -->
        <app-modal
          [isOpen]="showModalMd()"
          size="md"
          [hasFooter]="true"
          (closed)="showModalMd.set(false)"
        >
          <h2 modal-header>Medium Modal</h2>
          <p>This is a medium-sized modal with more content.</p>
          <p>It can contain multiple paragraphs and other elements.</p>
          <app-progress-bar
            label="Your Progress"
            [current]="75"
            [max]="100"
            variant="xp"
          />
          <div modal-footer>
            <app-button variant="secondary" (clicked)="showModalMd.set(false)">
              Cancel
            </app-button>
            <app-button (clicked)="showModalMd.set(false)">
              Confirm
            </app-button>
          </div>
        </app-modal>

        <!-- Large Modal -->
        <app-modal
          [isOpen]="showModalLg()"
          size="lg"
          [hasFooter]="true"
          (closed)="showModalLg.set(false)"
        >
          <h2 modal-header>Large Modal</h2>
          <p>This is a large modal that can contain substantial content.</p>
          
          <app-card>
            <h3>Example Card Inside Modal</h3>
            <p>Modals can contain any components, including cards.</p>
          </app-card>

          <div style="margin-top: 1rem;">
            <app-badge variant="success" [showDot]="true">Ready</app-badge>
            <app-badge variant="in-progress" [showDot]="true">Processing</app-badge>
          </div>

          <div modal-footer>
            <app-button variant="ghost" (clicked)="showModalLg.set(false)">
              Cancel
            </app-button>
            <app-button variant="danger">
              Delete
            </app-button>
            <app-button (clicked)="showModalLg.set(false)">
              Save Changes
            </app-button>
          </div>
        </app-modal>
      </section>
    </div>
  `,
  styles: [`
    .showcase {
      min-height: 100vh;
      background: var(--bg-primary);
      padding: 2rem;
    }

    .showcase-header {
      margin-bottom: 3rem;
      text-align: center;

      h1 {
        font-size: 3rem;
        background: linear-gradient(135deg, var(--accent-purple), var(--accent-cyan));
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        margin-bottom: 0.5rem;
      }

      p {
        color: var(--text-secondary);
        font-size: 1.125rem;
      }
    }

    .showcase-section {
      background: var(--bg-card);
      border: 1px solid var(--border-accent);
      border-radius: 16px;
      padding: 2rem;
      margin-bottom: 2rem;

      h2 {
        font-size: 1.75rem;
        color: var(--text-primary);
        margin-bottom: 1.5rem;
        padding-bottom: 0.75rem;
        border-bottom: 1px solid var(--border-accent);
      }
    }

    .showcase-group {
      margin-bottom: 2rem;

      &:last-child {
        margin-bottom: 0;
      }

      h3 {
        font-size: 1rem;
        color: var(--text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.1em;
        margin-bottom: 1rem;
      }
    }

    .showcase-row {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      align-items: center;
    }

    .showcase-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.5rem;
    }

    @media (max-width: 640px) {
      .showcase {
        padding: 1rem;
      }

      .showcase-section {
        padding: 1rem;
      }
    }
  `]
})
export class ComponentShowcaseComponent {
  showModalSm = signal(false);
  showModalMd = signal(false);
  showModalLg = signal(false);
}
