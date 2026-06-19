import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface MenuOption {
  icon: string;
  label: string;
  route: string;
  subLabel: string;
}

@Component({
  selector: 'gifs-side-menu-options',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './side-menu-options.component.html',
})
export class SideMenuOptionsComponent {

  menuOptions: MenuOption[] = [
    {
      icon: 'fa-solid fa-chart-line',
      label: 'Países',
      subLabel: 'Info de países',
      route: '/country',
    },
    {
      icon: 'fa-solid fa-magnifying-glass',
      label: 'Buscador GIFS',
      subLabel: 'Buscar gifs',
      route: '/gifs',
    },
  ];
}
