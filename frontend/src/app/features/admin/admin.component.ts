import { Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';

import { AdminService } from './admin.service';
import { ToastService } from '../../shared/services/toast.service';
import { LoaderComponent } from '../../shared/components/loader/loader.component';
import { NavbarComponent } from '../../shared/components/navbar/navbar.component';
import {
  CatalogAdminItemDto,
  CreateCatalogItemRequest,
  KycActionRequest,
  KycReviewDto,
  TicketDetailDto,
  TicketSummaryDto,
} from '../../shared/models/admin.model';
import { UserDto } from '../../shared/models/auth.model';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [DatePipe, DecimalPipe, FormsModule, ReactiveFormsModule, LoaderComponent, NavbarComponent],
  template: `
    <div class="flex flex-col h-screen bg-surface-950 text-zinc-100">
      <app-navbar />
      <main class="flex-1 overflow-y-auto bg-gradient-to-b from-surface-900 to-surface-950">
        <div class="p-6 max-w-6xl mx-auto space-y-8">
          <div class="space-y-4">
            <h1 class="text-2xl font-display font-bold text-zinc-100">Admin Panel</h1>
            <div class="sticky top-0 z-20 -mx-2 px-2 py-3 bg-surface-950/90 backdrop-blur border border-zinc-800 rounded-xl">
              <div class="flex gap-2 overflow-x-auto pb-1">
                @for (item of topNavItems; track item.id) {
                  <button
                    type="button"
                    (click)="scrollToSection(item.id)"
                    class="shrink-0 px-3 py-1.5 rounded-lg text-xs font-semibold border transition-colors"
                    [class.bg-accent/20]="item.id === activeSection"
                    [class.text-accent]="item.id === activeSection"
                    [class.border-accent/40]="item.id === activeSection"
                    [class.bg-zinc-900]="item.id !== activeSection"
                    [class.text-zinc-300]="item.id !== activeSection"
                    [class.border-zinc-700]="item.id !== activeSection"
                  >
                    {{ item.label }}
                  </button>
                }
              </div>
            </div>
          </div>

            <!-- ── KYC Reviews ── -->
            <div id="kyc" class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-6">
              <div class="flex items-center justify-between mb-4">
                <h2 class="text-lg font-semibold text-zinc-100">Pending KYC Reviews</h2>
                <span class="text-xs text-zinc-500">{{ kycList.length }} open</span>
              </div>
              <app-loader [show]="loadingKyc" />

              @if (!loadingKyc) {
                @if (kycList.length === 0) {
                  <p class="text-zinc-500 text-sm text-center py-6">No pending KYC reviews</p>
                } @else {
                  <div class="overflow-x-auto">
                    <table class="w-full text-sm">
                      <thead>
                        <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                          <th class="pb-3 font-medium">User</th>
                          <th class="pb-3 font-medium">Document</th>
                          <th class="pb-3 font-medium">Status</th>
                          <th class="pb-3 font-medium">Submitted</th>
                          <th class="pb-3 font-medium">Actions</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-zinc-800">
                        @for (doc of kycList; track doc.id) {
                          <tr class="hover:bg-zinc-800/70">
                            <td class="py-3">
                              <p class="text-zinc-200 text-sm font-medium">{{ doc.userFullName }}</p>
                              <p class="text-zinc-500 text-xs">{{ doc.userEmail }}</p>
                            </td>
                            <td class="py-3">
                              <span class="px-2 py-0.5 bg-amber-500/20 text-amber-300 rounded-full text-xs font-medium">
                                {{ doc.documentType }}
                              </span>
                              <p class="text-zinc-500 text-xs mt-0.5 font-mono">{{ doc.documentNumber }}</p>
                            </td>
                            <td class="py-3">
                              <span
                                class="px-2 py-0.5 rounded-full text-xs font-semibold"
                                [class.bg-yellow-500/20]="doc.status === 'Pending'"
                                [class.text-yellow-300]="doc.status === 'Pending'"
                                [class.bg-emerald-500/20]="doc.status === 'Approved'"
                                [class.text-emerald-300]="doc.status === 'Approved'"
                                [class.bg-rose-500/20]="doc.status === 'Rejected'"
                                [class.text-rose-300]="doc.status === 'Rejected'"
                              >{{ doc.status }}</span>
                            </td>
                            <td class="py-3 text-zinc-400 text-xs">{{ doc.submittedAt | date:'dd MMM, HH:mm' }}</td>
                            <td class="py-3">
                              @if (doc.status === 'Pending') {
                                <div class="flex gap-2">
                                  <button
                                    (click)="openApprove(doc)"
                                    class="px-3 py-1 bg-green-600 hover:bg-green-700 text-white text-xs rounded-lg font-medium"
                                  >Approve</button>
                                  <button
                                    (click)="openReject(doc)"
                                    class="px-3 py-1 bg-red-600 hover:bg-red-700 text-white text-xs rounded-lg font-medium"
                                  >Reject</button>
                                </div>
                              } @else {
                                <span class="text-zinc-600 text-xs">{{ doc.adminNote || '-' }}</span>
                              }
                            </td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  </div>
                }
              }
            </div>

            <!-- ── Support Tickets ── -->
            <div id="tickets" class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-6">
              <h2 class="text-lg font-semibold text-zinc-100 mb-4">Support Tickets</h2>
              <app-loader [show]="loadingTickets" />

              @if (!loadingTickets) {
                @if (tickets.length === 0) {
                  <p class="text-zinc-500 text-sm text-center py-6">No tickets found</p>
                } @else {
                  <div class="overflow-x-auto">
                    <table class="w-full text-sm">
                      <thead>
                        <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                          <th class="pb-3 font-medium">Ticket</th>
                          <th class="pb-3 font-medium">Meta</th>
                          <th class="pb-3 font-medium">Status</th>
                          <th class="pb-3 font-medium">Created</th>
                          <th class="pb-3 font-medium">Actions</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-zinc-800">
                        @for (ticket of tickets; track ticket.id) {
                          <tr class="hover:bg-zinc-800/70">
                            <td class="py-3 min-w-60">
                              <p class="text-zinc-100 text-sm font-medium truncate max-w-52">{{ ticket.subject }}</p>
                              <p class="text-zinc-500 text-xs mt-0.5">#{{ ticket.ticketNumber }}</p>
                            </td>
                            <td class="py-3 text-zinc-400 text-xs">
                              <div>{{ ticket.category }}</div>
                              <div>{{ ticket.priority }} priority</div>
                              <div>{{ ticket.replyCount }} {{ ticket.replyCount === 1 ? 'reply' : 'replies' }}</div>
                            </td>
                            <td class="py-3">
                              <span
                                class="px-2 py-0.5 rounded-full text-xs font-semibold"
                                [class.bg-yellow-500/20]="ticket.status === 'Open'"
                                [class.text-yellow-300]="ticket.status === 'Open'"
                                [class.bg-amber-500/20]="ticket.status === 'InProgress'"
                                [class.text-amber-300]="ticket.status === 'InProgress'"
                                [class.bg-fuchsia-500/20]="ticket.status === 'Responded'"
                                [class.text-fuchsia-300]="ticket.status === 'Responded'"
                                [class.bg-emerald-500/20]="ticket.status === 'Resolved'"
                                [class.text-emerald-300]="ticket.status === 'Resolved'"
                                [class.bg-zinc-700]="ticket.status === 'Closed'"
                                [class.text-zinc-300]="ticket.status === 'Closed'"
                              >{{ ticket.status }}</span>
                            </td>
                            <td class="py-3 text-zinc-400 text-xs">{{ ticket.createdAt | date:'dd MMM, HH:mm' }}</td>
                            <td class="py-3">
                              <button
                                (click)="openTicket(ticket)"
                                [disabled]="ticketDetailLoadingId === ticket.id"
                                class="px-3 py-1 bg-accent hover:bg-accent-hover text-white text-xs rounded-lg font-medium disabled:opacity-50"
                              >{{ ticketDetailLoadingId === ticket.id ? 'Loading...' : 'View' }}</button>
                            </td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  </div>
                }
              }
            </div>

            <!-- ── Catalog Items ── -->
            <div id="catalog" class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-6">
              <div class="flex items-center justify-between mb-4">
                <h2 class="text-lg font-semibold text-zinc-100">Reward Catalog</h2>
                <button
                  (click)="showCreateCatalog = true"
                  class="px-4 py-2 bg-accent hover:bg-accent-hover text-white rounded-lg font-medium text-sm"
                >+ Add Catalog Item</button>
              </div>
              <app-loader [show]="loadingCatalog" />

              @if (!loadingCatalog) {
                @if (catalogItems.length === 0) {
                  <p class="text-zinc-500 text-sm text-center py-6">No catalog items yet. Add items for users to redeem.</p>
                } @else {
                  <div class="overflow-x-auto">
                    <table class="w-full text-sm">
                      <thead>
                        <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                          <th class="pb-3 font-medium">Item</th>
                          <th class="pb-3 font-medium">Category</th>
                          <th class="pb-3 font-medium">Points Cost</th>
                          <th class="pb-3 font-medium">Status</th>
                          <th class="pb-3 font-medium">Actions</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-zinc-800">
                        @for (item of catalogItems; track item.id) {
                          <tr class="hover:bg-zinc-800/70">
                            <td class="py-3">
                              <p class="text-zinc-100 text-sm font-medium">{{ item.name }}</p>
                              <p class="text-zinc-500 text-xs mt-0.5 max-w-60 truncate">{{ item.description }}</p>
                            </td>
                            <td class="py-3">
                              <span class="px-2 py-0.5 bg-zinc-700 text-zinc-300 rounded-full text-xs">{{ item.category }}</span>
                            </td>
                            <td class="py-3">
                              <span class="text-amber-300 font-semibold">{{ item.pointsCost | number }} pts</span>
                            </td>
                            <td class="py-3">
                              <span
                                class="px-2 py-0.5 rounded-full text-xs font-semibold"
                                [class.bg-emerald-500/20]="item.isActive"
                                [class.text-emerald-300]="item.isActive"
                                [class.bg-zinc-700]="!item.isActive"
                                [class.text-zinc-400]="!item.isActive"
                              >{{ item.isActive ? 'Active' : 'Inactive' }}</span>
                            </td>
                            <td class="py-3">
                              <button
                                type="button"
                                (click)="openCatalogDetails(item)"
                                class="px-3 py-1 bg-zinc-800 hover:bg-zinc-700 text-zinc-200 text-xs rounded-lg font-medium border border-zinc-700"
                              >
                                View
                              </button>
                            </td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  </div>
                }
              }
            </div>

            <!-- ── All Users ── -->
            <div id="users" class="bg-zinc-900/80 border border-zinc-800 rounded-xl shadow p-6">
              <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-4">
                <h2 class="text-lg font-semibold text-zinc-100">All Users</h2>
                <input
                  type="text"
                  [(ngModel)]="userSearchTerm"
                  placeholder="Search by email, phone, role, or user id"
                  class="w-full sm:w-80 px-3 py-2 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-accent"
                />
              </div>
              <app-loader [show]="loadingUsers" />

              @if (!loadingUsers) {
                @if (users.length === 0) {
                  <p class="text-zinc-500 text-sm text-center py-6">No users found</p>
                } @else if (filteredUsers.length === 0) {
                  <p class="text-zinc-500 text-sm text-center py-6">No users matched your search.</p>
                } @else {
                  <div class="overflow-x-auto">
                    <table class="w-full text-sm">
                      <thead>
                        <tr class="border-b border-zinc-700 text-zinc-400 text-left">
                          <th class="pb-3 font-medium">User</th>
                          <th class="pb-3 font-medium">Phone</th>
                          <th class="pb-3 font-medium">Role</th>
                          <th class="pb-3 font-medium">Status</th>
                          <th class="pb-3 font-medium">Joined</th>
                          <th class="pb-3 font-medium">Actions</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-zinc-800">
                        @for (user of filteredUsers; track user.id) {
                          <tr class="hover:bg-zinc-800/70">
                            <td class="py-3">
                              <p class="text-zinc-100 text-sm">{{ user.email }}</p>
                              <p class="text-zinc-500 text-xs">User {{ user.id.slice(0, 8) }}</p>
                            </td>
                            <td class="py-3 text-zinc-400">{{ user.phone || '-' }}</td>
                            <td class="py-3">
                              <span
                                class="px-2 py-0.5 rounded-full text-xs font-semibold"
                                [class.bg-fuchsia-500/20]="user.role === 'Admin'"
                                [class.text-fuchsia-300]="user.role === 'Admin'"
                                [class.bg-zinc-700]="user.role === 'User'"
                                [class.text-zinc-200]="user.role === 'User'"
                              >{{ user.role }}</span>
                            </td>
                            <td class="py-3">
                              <span
                                class="px-2 py-0.5 rounded-full text-xs font-semibold"
                                [class.bg-emerald-500/20]="user.isActive"
                                [class.text-emerald-300]="user.isActive"
                                [class.bg-rose-500/20]="!user.isActive"
                                [class.text-rose-300]="!user.isActive"
                              >{{ user.isActive ? 'Active' : 'Inactive' }}</span>
                            </td>
                            <td class="py-3 text-zinc-500 text-xs">{{ user.createdAt | date:'dd MMM yyyy' }}</td>
                            <td class="py-3">
                              <button
                                type="button"
                                (click)="toggleUserStatus(user)"
                                [disabled]="userStatusLoadingId === user.id"
                                class="px-3 py-1 rounded-lg text-xs font-medium border disabled:opacity-60 disabled:cursor-not-allowed"
                                [class.bg-rose-600]="user.isActive"
                                [class.hover:bg-rose-700]="user.isActive"
                                [class.border-rose-500]="user.isActive"
                                [class.text-white]="user.isActive"
                                [class.bg-emerald-600]="!user.isActive"
                                [class.hover:bg-emerald-700]="!user.isActive"
                                [class.border-emerald-500]="!user.isActive"
                              >
                                {{
                                  userStatusLoadingId === user.id
                                    ? 'Updating...'
                                    : user.isActive
                                      ? 'Set Inactive'
                                      : 'Set Active'
                                }}
                              </button>
                            </td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  </div>
                }
              }
            </div>
          </div>
        </main>
    </div>

    <!-- ── Approve KYC Modal ── -->
    @if (approveDoc) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-xl w-full max-w-sm p-6">
          <h3 class="text-lg font-bold text-zinc-100 mb-2">Approve KYC</h3>
          <p class="text-zinc-400 text-sm mb-4">
            <strong>{{ approveDoc.documentType }}</strong> — {{ approveDoc.userFullName }}
          </p>
          <form [formGroup]="approveForm" (ngSubmit)="submitApprove()" novalidate class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Admin Note (optional)</label>
              <textarea
                formControlName="adminNote"
                rows="3"
                placeholder="Add approval notes..."
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-green-500 resize-none"
              ></textarea>
            </div>
            <div class="flex gap-3">
              <button type="button" (click)="approveDoc = null" class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-3 rounded-lg hover:bg-zinc-800">
                Cancel
              </button>
              <button type="submit" [disabled]="actionLoading" class="flex-1 bg-green-600 hover:bg-green-700 text-white font-semibold py-3 rounded-lg disabled:opacity-60">
                {{ actionLoading ? 'Approving...' : 'Approve' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- ── Reject KYC Modal ── -->
    @if (rejectDoc) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-xl w-full max-w-sm p-6">
          <h3 class="text-lg font-bold text-zinc-100 mb-2">Reject KYC</h3>
          <p class="text-zinc-400 text-sm mb-4">
            <strong>{{ rejectDoc.documentType }}</strong> — {{ rejectDoc.userFullName }}
          </p>
          <form [formGroup]="rejectForm" (ngSubmit)="submitReject()" novalidate class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Reason <span class="text-red-500">*</span></label>
              <textarea
                formControlName="adminNote"
                rows="3"
                placeholder="Explain why this document is rejected..."
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-red-500 resize-none"
                [class.border-red-500]="rejectSubmitted && rejectForm.controls.adminNote.invalid"
              ></textarea>
              @if (rejectSubmitted && rejectForm.controls.adminNote.invalid) {
                <p class="text-red-500 text-xs mt-1">Reason is required</p>
              }
            </div>
            <div class="flex gap-3">
              <button type="button" (click)="rejectDoc = null" class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-3 rounded-lg hover:bg-zinc-800">
                Cancel
              </button>
              <button type="submit" [disabled]="actionLoading" class="flex-1 bg-red-600 hover:bg-red-700 text-white font-semibold py-3 rounded-lg disabled:opacity-60">
                {{ actionLoading ? 'Rejecting...' : 'Reject' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- ── View / Reply Ticket Modal ── -->
    @if (viewTicket) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-xl w-full max-w-lg p-6">
          <div class="flex justify-between items-center mb-4">
            <h3 class="text-lg font-bold text-zinc-100">Ticket #{{ viewTicket.ticketNumber }}</h3>
            <button (click)="closeView()" class="text-zinc-500 hover:text-zinc-300 text-2xl leading-none">&times;</button>
          </div>
          <p class="text-zinc-200 text-sm font-medium mb-1">{{ viewTicket.subject }}</p>
          <p class="text-zinc-500 text-xs mb-3">{{ viewTicket.category }} &bull; {{ viewTicket.priority }} priority &bull; {{ viewTicket.userEmail }}</p>

          <div class="bg-zinc-800 rounded-lg p-3 mb-3 text-sm text-zinc-300">
            {{ viewTicket.description }}
          </div>

          @if (viewTicket.adminReply) {
            <div class="bg-accent/10 border border-accent/20 rounded-lg p-3 mb-3 text-sm">
              <p class="text-xs text-accent mb-1">Admin Reply
                @if (viewTicket.respondedAt) {
                  — {{ viewTicket.respondedAt | date:'dd MMM, HH:mm' }}
                }
              </p>
              <p class="text-zinc-300">{{ viewTicket.adminReply }}</p>
            </div>
          }

          @if (viewTicket.status !== 'Closed') {
            <form [formGroup]="replyForm" (ngSubmit)="submitReply()" novalidate class="space-y-3 mt-4">
              <div>
                <label class="block text-sm font-medium text-zinc-300 mb-1">Reply</label>
                <textarea
                  formControlName="reply"
                  rows="4"
                  placeholder="Type your response..."
                  class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent resize-none"
                  [class.border-red-500]="replySubmitted && replyForm.controls.reply.invalid"
                ></textarea>
                @if (replySubmitted && replyForm.controls.reply.invalid) {
                  <p class="text-red-500 text-xs mt-1">Reply is required</p>
                }
              </div>
              <div class="flex gap-2">
                <button
                  type="button"
                  (click)="closeView()"
                  class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-2.5 rounded-lg hover:bg-zinc-800 text-sm"
                >Cancel</button>
                <button
                  type="button"
                  (click)="closeTicket()"
                  [disabled]="closeLoading || replyLoading"
                  class="px-4 py-2.5 bg-zinc-700 hover:bg-zinc-600 text-zinc-200 font-semibold rounded-lg disabled:opacity-50 text-sm"
                >{{ closeLoading ? 'Closing...' : 'Close Ticket' }}</button>
                <button
                  type="submit"
                  [disabled]="replyLoading || closeLoading"
                  class="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold py-2.5 rounded-lg disabled:opacity-60 text-sm"
                >{{ replyLoading ? 'Sending...' : 'Send Reply' }}</button>
              </div>
            </form>
          } @else {
            <div class="flex gap-2 mt-4">
              <button (click)="closeView()" class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-2.5 rounded-lg hover:bg-zinc-800 text-sm">Close</button>
            </div>
            <p class="text-zinc-500 text-xs text-center mt-2">This ticket has been closed.</p>
          }
        </div>
      </div>
    }

    <!-- ── Create Catalog Item Modal ── -->
    @if (showCreateCatalog) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-xl w-full max-w-md p-6">
          <div class="flex justify-between items-center mb-5">
            <h3 class="text-lg font-bold text-zinc-100">Add Catalog Item</h3>
            <button (click)="showCreateCatalog = false" class="text-zinc-500 hover:text-zinc-300 text-2xl leading-none">&times;</button>
          </div>
          <form [formGroup]="catalogForm" (ngSubmit)="submitCatalogItem()" novalidate class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Item Name</label>
              <input
                type="text"
                formControlName="name"
                placeholder="e.g. Amazon Gift Card ₹500"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent"
                [class.border-red-500]="catalogSubmitted && catalogForm.controls.name.invalid"
              />
              @if (catalogSubmitted && catalogForm.controls.name.invalid) {
                <p class="text-red-500 text-xs mt-1">Item name is required</p>
              }
            </div>
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Description</label>
              <textarea
                formControlName="description"
                rows="2"
                placeholder="Brief description of the reward..."
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent resize-none"
              ></textarea>
            </div>
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Category</label>
              <select
                formControlName="category"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent"
              >
                <option value="GiftCard">Gift Card</option>
                <option value="Cashback">Cashback</option>
                <option value="Voucher">Voucher</option>
                <option value="Merchandise">Merchandise</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-zinc-300 mb-1">Points Cost</label>
              <input
                type="number"
                formControlName="pointsCost"
                min="1"
                placeholder="e.g. 500"
                class="w-full px-4 py-2.5 border border-zinc-700 bg-zinc-950 rounded-lg text-sm text-zinc-100 focus:outline-none focus:ring-2 focus:ring-accent"
                [class.border-red-500]="catalogSubmitted && catalogForm.controls.pointsCost.invalid"
              />
              @if (catalogSubmitted && catalogForm.controls.pointsCost.invalid) {
                <p class="text-red-500 text-xs mt-1">Points cost must be at least 1</p>
              }
            </div>
            <div class="flex gap-3 pt-2">
              <button type="button" (click)="showCreateCatalog = false" class="flex-1 border border-zinc-700 text-zinc-300 font-semibold py-3 rounded-lg hover:bg-zinc-800">Cancel</button>
              <button type="submit" [disabled]="catalogLoading" class="flex-1 bg-accent hover:bg-accent-hover text-white font-semibold py-3 rounded-lg disabled:opacity-60">
                {{ catalogLoading ? 'Adding...' : 'Add Item' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }

    <!-- ── Catalog Detail Modal ── -->
    @if (catalogDetailItem) {
      <div class="fixed inset-0 bg-black/50 z-40 flex items-center justify-center p-4">
        <div class="bg-zinc-900 border border-zinc-700 rounded-2xl shadow-xl w-full max-w-lg p-6">
          <div class="flex justify-between items-center mb-5">
            <h3 class="text-lg font-bold text-zinc-100">Catalog Item Details</h3>
            <button (click)="closeCatalogDetails()" class="text-zinc-500 hover:text-zinc-300 text-2xl leading-none">&times;</button>
          </div>

          <div class="space-y-4">
            <div>
              <p class="text-xs uppercase tracking-wide text-zinc-500">Item Name</p>
              <p class="text-zinc-100 font-semibold text-base">{{ catalogDetailItem.name }}</p>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <p class="text-xs uppercase tracking-wide text-zinc-500">Category</p>
                <p class="text-zinc-200">{{ catalogDetailItem.category }}</p>
              </div>
              <div>
                <p class="text-xs uppercase tracking-wide text-zinc-500">Points Cost</p>
                <p class="text-amber-300 font-semibold">{{ catalogDetailItem.pointsCost | number }} pts</p>
              </div>
              <div>
                <p class="text-xs uppercase tracking-wide text-zinc-500">Status</p>
                <span
                  class="inline-flex mt-1 px-2 py-0.5 rounded-full text-xs font-semibold"
                  [class.bg-emerald-500/20]="catalogDetailItem.isActive"
                  [class.text-emerald-300]="catalogDetailItem.isActive"
                  [class.bg-zinc-700]="!catalogDetailItem.isActive"
                  [class.text-zinc-400]="!catalogDetailItem.isActive"
                >{{ catalogDetailItem.isActive ? 'Active' : 'Inactive' }}</span>
              </div>
            </div>

            <div>
              <p class="text-xs uppercase tracking-wide text-zinc-500">Description</p>
              <p class="text-zinc-300 text-sm whitespace-pre-wrap">{{ catalogDetailItem.description || '-' }}</p>
            </div>
          </div>

          <div class="mt-6">
            <button
              type="button"
              (click)="closeCatalogDetails()"
              class="w-full border border-zinc-700 text-zinc-300 font-semibold py-2.5 rounded-lg hover:bg-zinc-800"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    }
  `,
})
export class AdminComponent implements OnInit {
  readonly topNavItems = [
    { id: 'kyc', label: 'KYC Reviews' },
    { id: 'tickets', label: 'Support Tickets' },
    { id: 'catalog', label: 'Catalog' },
    { id: 'users', label: 'Users' },
  ] as const;
  activeSection: (typeof this.topNavItems)[number]['id'] = 'kyc';

  loadingKyc = false;
  loadingTickets = false;
  loadingUsers = false;
  loadingCatalog = false;
  userStatusLoadingId: string | null = null;

  kycList: KycReviewDto[] = [];
  tickets: TicketSummaryDto[] = [];
  users: UserDto[] = [];
  userSearchTerm = '';
  catalogItems: CatalogAdminItemDto[] = [];

  actionLoading = false;
  replyLoading = false;
  closeLoading = false;
  replySubmitted = false;
  rejectSubmitted = false;
  catalogSubmitted = false;
  catalogLoading = false;
  ticketDetailLoadingId: string | null = null;
  showCreateCatalog = false;

  approveDoc: KycReviewDto | null = null;
  rejectDoc: KycReviewDto | null = null;
  viewTicket: TicketDetailDto | null = null;
  catalogDetailItem: CatalogAdminItemDto | null = null;

  readonly approveForm;
  readonly rejectForm;
  readonly replyForm;
  readonly catalogForm;

  constructor(
    fb: FormBuilder,
    private readonly adminService: AdminService,
    private readonly toastService: ToastService,
  ) {
    this.approveForm = fb.nonNullable.group({ adminNote: [''] });
    this.rejectForm = fb.nonNullable.group({ adminNote: ['', Validators.required] });
    this.replyForm = fb.nonNullable.group({ reply: ['', Validators.required] });
    this.catalogForm = fb.nonNullable.group({
      name: ['', Validators.required],
      description: [''],
      category: ['GiftCard'],
      pointsCost: [100, [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    this.loadKyc();
    this.loadTickets();
    this.loadUsers();
    this.loadCatalog();
  }

  scrollToSection(sectionId: (typeof this.topNavItems)[number]['id']): void {
    this.activeSection = sectionId;
    const section = document.getElementById(sectionId);
    if (!section) return;

    section.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  loadKyc(): void {
    this.loadingKyc = true;
    this.adminService.getPendingKyc().subscribe({
      next: (result) => {
        this.kycList = result ?? [];
        this.loadingKyc = false;
      },
      error: (err: { message?: string }) => {
        this.loadingKyc = false;
        this.toastService.error(err.message ?? 'Failed to load KYC reviews.');
      },
    });
  }

  loadUsers(): void {
    this.loadingUsers = true;
    this.adminService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.loadingUsers = false;
      },
      error: (err: { message?: string }) => {
        this.loadingUsers = false;
        this.toastService.error(err.message ?? 'Failed to load users.');
      },
    });
  }

  loadTickets(): void {
    this.loadingTickets = true;
    this.adminService.getTickets().subscribe({
      next: (result) => {
        this.tickets = result ?? [];
        this.loadingTickets = false;
      },
      error: (err: { message?: string }) => {
        this.loadingTickets = false;
        this.toastService.error(err.message ?? 'Failed to load tickets.');
      },
    });
  }

  loadCatalog(): void {
    this.loadingCatalog = true;
    this.adminService.getCatalogItems().subscribe({
      next: (result) => {
        this.catalogItems = result ?? [];
        this.loadingCatalog = false;
      },
      error: () => (this.loadingCatalog = false),
    });
  }

  openApprove(doc: KycReviewDto): void {
    this.approveDoc = doc;
    this.approveForm.reset();
  }

  openReject(doc: KycReviewDto): void {
    this.rejectDoc = doc;
    this.rejectSubmitted = false;
    this.rejectForm.reset();
  }

  submitApprove(): void {
    if (!this.approveDoc) return;
    this.actionLoading = true;
    const payload: KycActionRequest = { adminNote: this.approveForm.value.adminNote || undefined };
    this.adminService.approveKyc(this.approveDoc.id, payload).subscribe({
      next: () => {
        this.toastService.success('KYC approved!');
        this.approveDoc = null;
        this.actionLoading = false;
        this.loadKyc();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Approval failed');
        this.actionLoading = false;
      },
    });
  }

  submitReject(): void {
    this.rejectSubmitted = true;
    if (this.rejectForm.invalid || !this.rejectDoc) return;
    this.actionLoading = true;
    const payload: KycActionRequest = { adminNote: this.rejectForm.value.adminNote };
    this.adminService.rejectKyc(this.rejectDoc.id, payload).subscribe({
      next: () => {
        this.toastService.success('KYC rejected.');
        this.rejectDoc = null;
        this.actionLoading = false;
        this.loadKyc();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Rejection failed');
        this.actionLoading = false;
      },
    });
  }

  openTicket(ticket: TicketSummaryDto): void {
    this.replySubmitted = false;
    this.replyForm.reset();
    this.ticketDetailLoadingId = ticket.id;
    this.adminService.getTicket(ticket.id).subscribe({
      next: (detail) => {
        this.viewTicket = detail;
        this.ticketDetailLoadingId = null;
      },
      error: (err: { message?: string }) => {
        this.ticketDetailLoadingId = null;
        this.toastService.error(err.message ?? 'Failed to load ticket details.');
      },
    });
  }

  closeView(): void {
    this.viewTicket = null;
    this.replySubmitted = false;
    this.replyLoading = false;
    this.closeLoading = false;
    this.replyForm.reset();
  }

  submitReply(): void {
    this.replySubmitted = true;
    if (this.replyForm.invalid || !this.viewTicket) return;
    this.replyLoading = true;
    this.adminService.replyToTicket(this.viewTicket.id, this.replyForm.value.reply!).subscribe({
      next: () => {
        this.toastService.success('Reply sent successfully.');
        this.replyLoading = false;
        this.closeView();
        this.loadTickets();
      },
      error: (err: { message?: string }) => {
        this.replyLoading = false;
        this.toastService.error(err.message ?? 'Failed to send reply.');
      },
    });
  }

  closeTicket(): void {
    if (!this.viewTicket) return;
    this.closeLoading = true;
    this.adminService.closeTicket(this.viewTicket.id).subscribe({
      next: () => {
        this.toastService.success('Ticket closed.');
        this.closeLoading = false;
        this.closeView();
        this.loadTickets();
      },
      error: (err: { message?: string }) => {
        this.closeLoading = false;
        this.toastService.error(err.message ?? 'Failed to close ticket.');
      },
    });
  }

  submitCatalogItem(): void {
    this.catalogSubmitted = true;
    if (this.catalogForm.invalid) return;
    this.catalogLoading = true;
    const v = this.catalogForm.getRawValue();
    const payload: CreateCatalogItemRequest = {
      name: v.name,
      description: v.description,
      category: v.category,
      pointsCost: v.pointsCost,
    };
    this.adminService.createCatalogItem(payload).subscribe({
      next: () => {
        this.toastService.success('Catalog item added!');
        this.showCreateCatalog = false;
        this.catalogLoading = false;
        this.catalogSubmitted = false;
        this.catalogForm.reset({ category: 'GiftCard', pointsCost: 100 });
        this.loadCatalog();
      },
      error: (err: { message?: string }) => {
        this.toastService.error(err.message ?? 'Failed to add catalog item.');
        this.catalogLoading = false;
      },
    });
  }

  openCatalogDetails(item: CatalogAdminItemDto): void {
    this.catalogDetailItem = item;
  }

  closeCatalogDetails(): void {
    this.catalogDetailItem = null;
  }

  get filteredUsers(): UserDto[] {
    const query = this.userSearchTerm.trim().toLowerCase();
    if (!query) return this.users;

    return this.users.filter((user) =>
      user.email.toLowerCase().includes(query) ||
      (user.phone ?? '').toLowerCase().includes(query) ||
      (user.role ?? '').toLowerCase().includes(query) ||
      user.id.toLowerCase().includes(query),
    );
  }

  toggleUserStatus(user: UserDto): void {
    if (this.userStatusLoadingId) return;

    const nextStatus = !user.isActive;
    this.userStatusLoadingId = user.id;
    this.adminService.updateUserStatus(user.id, nextStatus).subscribe({
      next: (updatedUser) => {
        this.users = this.users.map((existing) =>
          existing.id === updatedUser.id ? updatedUser : existing,
        );
        this.userStatusLoadingId = null;
        this.toastService.success(nextStatus ? 'User set to active.' : 'User set to inactive.');
      },
      error: (err: { message?: string }) => {
        this.userStatusLoadingId = null;
        this.toastService.error(err.message ?? 'Failed to update user status.');
      },
    });
  }
}
