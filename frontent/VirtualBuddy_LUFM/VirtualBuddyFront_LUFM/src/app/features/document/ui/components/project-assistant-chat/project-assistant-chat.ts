import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { catchError, exhaustMap, map, of, scan, startWith, Subject } from 'rxjs';
import { AskProjectAssistantUseCase } from '../../../application/ask-project-assistant.use-case';

type ChatTurn =
  | { readonly id: number; readonly question: string; readonly status: 'pending' }
  | {
      readonly id: number;
      readonly question: string;
      readonly status: 'success';
      readonly response: string;
    }
  | { readonly id: number; readonly question: string; readonly status: 'error' };

interface ChatRequest {
  readonly turnId: number;
  readonly question: string;
  readonly retry: boolean;
}

type ChatEvent =
  | { readonly type: 'pending'; readonly request: ChatRequest }
  | { readonly type: 'success'; readonly request: ChatRequest; readonly response: string }
  | { readonly type: 'error'; readonly request: ChatRequest };

@Component({
  selector: 'app-project-assistant-chat',
  imports: [MatButton, MatFormField, MatInput, MatLabel, ReactiveFormsModule],
  templateUrl: './project-assistant-chat.html',
  styleUrl: './project-assistant-chat.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectAssistantChat {
  private readonly askProjectAssistant = inject(AskProjectAssistantUseCase);
  private readonly requests = new Subject<ChatRequest>();
  private nextTurnId = 0;

  readonly projectId = input.required<string>();

  protected readonly questionControl = new FormControl('', { nonNullable: true });
  private readonly question = toSignal(this.questionControl.valueChanges, {
    initialValue: this.questionControl.value,
  });

  protected readonly turns = toSignal(
    this.requests.pipe(
      exhaustMap((request) =>
        this.askProjectAssistant.execute(this.projectId(), request.question).pipe(
          map(
            (response): ChatEvent => ({
              type: 'success',
              request,
              response,
            }),
          ),
          catchError(() => of<ChatEvent>({ type: 'error', request })),
          startWith<ChatEvent>({ type: 'pending', request }),
        ),
      ),
      scan((turns, event) => this.reduceTurns(turns, event), [] as readonly ChatTurn[]),
    ),
    { initialValue: [] as readonly ChatTurn[] },
  );

  protected readonly isPending = computed(() =>
    this.turns().some((turn) => turn.status === 'pending'),
  );
  protected readonly canSend = computed(() => Boolean(this.question().trim()) && !this.isPending());

  protected submit(event: SubmitEvent): void {
    event.preventDefault();

    if (!this.canSend()) return;

    const question = this.question().trim();
    this.questionControl.setValue('');
    this.requests.next({ turnId: this.nextTurnId++, question, retry: false });
  }

  protected retry(turn: ChatTurn): void {
    if (turn.status !== 'error' || this.isPending()) return;

    this.requests.next({ turnId: turn.id, question: turn.question, retry: true });
  }

  private reduceTurns(turns: readonly ChatTurn[], event: ChatEvent): readonly ChatTurn[] {
    const { request } = event;

    if (event.type === 'pending' && !request.retry) {
      return [...turns, { id: request.turnId, question: request.question, status: 'pending' }];
    }

    return turns.map((turn): ChatTurn => {
      if (turn.id !== request.turnId) return turn;

      if (event.type === 'success') {
        return {
          id: turn.id,
          question: turn.question,
          status: 'success',
          response: event.response,
        };
      }

      return {
        id: turn.id,
        question: turn.question,
        status: event.type === 'pending' ? 'pending' : 'error',
      };
    });
  }
}
