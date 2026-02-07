// SPDX-License-Identifier: GPL-3.0-or-later

declare global {
  interface JQuery {
    slick(method: string, ...args: any[]): JQuery;
    slick(options: any): JQuery;
  }
}

export {};
