import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [],
  template: `
    @if (show) {
      <div class="flex items-center justify-center py-12">
        <div
          class="w-10 h-10 border-4 border-accent border-t-zinc-700 rounded-full animate-spin"
        ></div>
      </div>
    }
  `,
})
export class LoaderComponent {
  @Input() show = false;
}
