import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Input, Output, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Observable, finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RobotStatus, RobotTelemetry, TeleopCommand } from '../../core/models/fleet.models';
import { RobotApiService } from '../../core/services/robot-api.service';

@Component({
  selector: 'app-recovery-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>Enter Recovery Mode</h2>
    <mat-dialog-content>
      Manual recovery mode can affect a physical robot. Confirm that the area is clear and
      recovery is authorized.
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="false">Cancel</button>
      <button mat-flat-button color="warn" [mat-dialog-close]="true">Confirm</button>
    </mat-dialog-actions>
  `,
})
export class RecoveryConfirmDialogComponent {}

@Component({
  selector: 'app-recovery-controls',
  imports: [CommonModule, MatButtonModule, MatDialogModule],
  templateUrl: './recovery-controls.component.html',
  styleUrl: './recovery-controls.component.scss',
})
export class RecoveryControlsComponent {
  @Input() robot?: RobotTelemetry;
  @Output() robotChanged = new EventEmitter<void>();

  error = '';
  commandInFlight = false;
  private lastTeleopAt = 0;
  private readonly destroyRef = inject(DestroyRef);
  private readonly teleopCooldownMs = 350;

  constructor(
    private readonly robots: RobotApiService,
    private readonly dialog: MatDialog,
  ) {}

  get canEnterRecovery(): boolean {
    return !this.commandInFlight && this.hasStatus(['Blocked', 'Faulted', 'EmergencyStopped', 'Offline']);
  }

  get canTeleop(): boolean {
    return !this.commandInFlight && this.robot?.status === 'RecoveryMode';
  }

  emergencyStop(): void {
    this.runCommand(() => this.robots.emergencyStop(this.robot!.robotId));
  }

  enterRecoveryMode(): void {
    if (!this.robot) {
      return;
    }

    const dialogRef = this.dialog.open(RecoveryConfirmDialogComponent, { width: '460px' });
    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (confirmed === true) {
          this.runCommand(() => this.robots.enterRecoveryMode(this.robot!.robotId));
        }
      });
  }

  exitRecoveryMode(): void {
    this.runCommand(() => this.robots.exitRecoveryMode(this.robot!.robotId));
  }

  teleop(command: TeleopCommand): void {
    const now = Date.now();
    if (now - this.lastTeleopAt < this.teleopCooldownMs) {
      return;
    }

    this.lastTeleopAt = now;
    this.runCommand(() => this.robots.sendTeleopCommand(this.robot!.robotId, command));
  }

  private hasStatus(statuses: RobotStatus[]): boolean {
    return !!this.robot && statuses.includes(this.robot.status);
  }

  private runCommand(command: () => Observable<void>): void {
    if (!this.robot) {
      return;
    }

    if (this.commandInFlight) {
      return;
    }

    this.error = '';
    this.commandInFlight = true;
    command()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.commandInFlight = false;
        }),
      )
      .subscribe({
        next: () => this.robotChanged.emit(),
        error: (error) => {
          this.error = error?.error?.message ?? 'Robot command failed.';
        },
      });
  }
}
