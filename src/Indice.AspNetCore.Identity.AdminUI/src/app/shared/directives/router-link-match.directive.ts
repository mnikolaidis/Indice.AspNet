import { AfterContentInit, ContentChildren, Directive, ElementRef, Input, OnChanges, OnDestroy, QueryList, Renderer2 } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkWithHref, Event } from '@angular/router';

import { untilDestroy } from '../operators/until-destroy';

export interface MatchExp {
    [classes: string]: string;
}

@Directive({
    selector: '[routerLinkMatch]'
})
export class RouterLinkMatchDirective implements OnDestroy, OnChanges, AfterContentInit {
    constructor(private router: Router, private renderer: Renderer2, private element: ElementRef) {
        router.events.pipe(untilDestroy(this)).subscribe((event: Event) => {
            if (event instanceof NavigationEnd) {
                this.currentRoute = (event as NavigationEnd).urlAfterRedirects;
            } else {
                this.currentRoute = this.router.url;
            }
            this.update();
        });
    }

    private currentRoute: string;
    private matchExp: MatchExp;
    @ContentChildren(RouterLink, { descendants: true }) public links: QueryList<RouterLink>;
    @ContentChildren(RouterLinkWithHref, { descendants: true }) public linksWithHrefs: QueryList<RouterLinkWithHref>;
    @Input() public routerLinkMatchOptions: { ignoreQueryParams?: boolean };

    @Input('routerLinkMatch')
    public set routerLinkMatch(matchExp: MatchExp) {
        if (matchExp && typeof matchExp === 'object') {
            this.matchExp = matchExp;
        } else {
            throw new TypeError(`Unexpected type '${typeof matchExp}' of value for input of routerLinkMatchDirective directive, expected 'object'.`);
        }
    }

    public ngOnChanges(): void {
        this.update();
    }

    public ngAfterContentInit(): void {
        this.links.changes.pipe(untilDestroy(this)).subscribe(() => this.update());
        this.linksWithHrefs.changes.pipe(untilDestroy(this)).subscribe(() => this.update());
        this.update();
    }

    private update(): void {
        if (!this.links || !this.linksWithHrefs || !this.router.navigated) {
            return;
        }
        Promise.resolve().then(() => {
            const matchExp = this.matchExp;
            Object.keys(matchExp).forEach((classes: string) => {
                if (matchExp[classes] && typeof matchExp[classes] === 'string') {
                    const regex = new RegExp(`^${matchExp[classes]}${(this.routerLinkMatchOptions && this.routerLinkMatchOptions.ignoreQueryParams === true ? '(?!\/)' : '$')}`, 'g');
                    if (this.currentRoute && this.currentRoute.match(regex)) {
                        this.toggleClass(classes, true);
                    } else {
                        this.toggleClass(classes, false);
                    }
                }
            });
        });
    }

    private toggleClass(classes: string, enabled: boolean): void {
        classes.split(/\s+/g).filter(classNames => !!classNames).forEach(className => {
            if (enabled) {
                this.renderer.addClass(this.element.nativeElement, className);
            } else {
                this.renderer.removeClass(this.element.nativeElement, className);
            }
        });
    }

    public ngOnDestroy(): void { }
}
