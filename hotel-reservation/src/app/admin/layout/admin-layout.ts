import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable } from 'rxjs';
import { map, shareReplay, take } from 'rxjs/operators';
import { AsyncPipe } from '@angular/common';
import { MatSidenav } from '@angular/material/sidenav';
import { AuthService } from '../../auth/services/auth.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
    selector: 'app-admin-layout',
    standalone: true,
    imports: [CommonModule, RouterModule, SharedModule, AsyncPipe],
    templateUrl: './admin-layout.html',
    styleUrl: './admin-layout.css'
})
export class AdminLayoutComponent {
    isHandset$: Observable<boolean>;

    constructor(
        public authService: AuthService,
        private breakpointObserver: BreakpointObserver
    ) {
        this.isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset)
            .pipe(
                map(result => result.matches),
                shareReplay()
            );
    }

    onLinkClick(drawer: MatSidenav) {
        this.isHandset$.pipe(take(1)).subscribe(isHandset => {
            if (isHandset) {
                drawer.close();
            }
        });
    }

    @ViewChild('drawer') drawer!: MatSidenav;

    toggleDrawer() {
        this.drawer.toggle();
    }

    get isAdmin(): boolean {
        return this.authService.currentUserValue?.role === 'Admin';
    }

    get isManager(): boolean {
        return this.authService.currentUserValue?.role === 'HotelManager';
    }

    get currentUser() {
        return this.authService.currentUserValue;
    }
}
