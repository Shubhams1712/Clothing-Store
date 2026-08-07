"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminApi } from "@/services/admin";
import type { Review } from "@/types/admin";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { Search, Check, X, MessageSquare, Trash2 } from "lucide-react";
import { toast } from "sonner";

export default function AdminReviewsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [filter, setFilter] = useState<string>("all");
  const [replyReview, setReplyReview] = useState<Review | null>(null);
  const [replyText, setReplyText] = useState("");
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const isApproved = filter === "approved" ? true : filter === "pending" ? false : undefined;

  const { data, isLoading } = useQuery({
    queryKey: ["admin-reviews", page, search, filter],
    queryFn: () => adminApi.reviews.list({ page, pageSize: 10, search, isApproved }),
  });

  const approveMutation = useMutation({
    mutationFn: (payload: { id: string; approved: boolean }) => adminApi.reviews.update(payload.id, { isApproved: payload.approved, isHidden: false }),
    onSuccess: () => { toast.success("Updated"); queryClient.invalidateQueries({ queryKey: ["admin-reviews"] }); },
    onError: () => { toast.error("Failed"); },
  });

  const replyMutation = useMutation({
    mutationFn: (payload: { id: string; adminReply: string }) => adminApi.reviews.reply(payload.id, { adminReply: payload.adminReply }),
    onSuccess: () => { toast.success("Reply added"); setReplyReview(null); setReplyText(""); queryClient.invalidateQueries({ queryKey: ["admin-reviews"] }); },
    onError: () => { toast.error("Failed"); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => adminApi.reviews.delete(id),
    onSuccess: () => { toast.success("Deleted"); setDeleteId(null); queryClient.invalidateQueries({ queryKey: ["admin-reviews"] }); },
    onError: () => { toast.error("Failed"); },
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Reviews</h1>
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center gap-4 mb-4">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="Search reviews..." value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} className="pl-9" />
            </div>
            <Tabs value={filter} onValueChange={v => { setFilter(v); setPage(1); }}>
              <TabsList>
                <TabsTrigger value="all">All</TabsTrigger>
                <TabsTrigger value="approved">Approved</TabsTrigger>
                <TabsTrigger value="pending">Pending</TabsTrigger>
              </TabsList>
            </Tabs>
          </div>
          {isLoading ? <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-12 w-full" />)}</div> : (
            <>
              <Table>
                <TableHeader><TableRow><TableHead>Product</TableHead><TableHead>Customer</TableHead><TableHead>Rating</TableHead><TableHead>Title</TableHead><TableHead>Status</TableHead><TableHead className="text-right">Actions</TableHead></TableRow></TableHeader>
                <TableBody>
                  {data?.items.map(r => (
                    <TableRow key={r.id}>
                      <TableCell className="font-medium">{r.productName}</TableCell>
                      <TableCell>{r.userName}</TableCell>
                      <TableCell>{"★".repeat(r.rating)}{"☆".repeat(5 - r.rating)}</TableCell>
                      <TableCell>{r.title}</TableCell>
                      <TableCell>
                        <Badge variant={r.isApproved ? "default" : "secondary"}>
                          {r.isApproved ? "Approved" : "Pending"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-1">
                          {!r.isApproved && (
                            <Button variant="ghost" size="icon" className="h-8 w-8 text-green-600" onClick={() => approveMutation.mutate({ id: r.id, approved: true })}>
                              <Check className="h-4 w-4" />
                            </Button>
                          )}
                          {r.isApproved && (
                            <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => approveMutation.mutate({ id: r.id, approved: false })}>
                              <X className="h-4 w-4" />
                            </Button>
                          )}
                          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => { setReplyReview(r); setReplyText(r.adminReply || ""); }}>
                            <MessageSquare className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive" onClick={() => setDeleteId(r.id)}>
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                  {data?.items.length === 0 && <TableRow><TableCell colSpan={6} className="text-center text-muted-foreground py-8">No reviews found</TableCell></TableRow>}
                </TableBody>
              </Table>
              {data && data.totalPages > 1 && (
                <div className="flex justify-between mt-4">
                  <p className="text-sm text-muted-foreground">Page {page} of {data.totalPages}</p>
                  <div className="flex gap-2">
                    <Button variant="outline" size="sm" disabled={page === 1} onClick={() => setPage(page - 1)}>Previous</Button>
                    <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage(page + 1)}>Next</Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!replyReview} onOpenChange={() => setReplyReview(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Reply to Review</DialogTitle></DialogHeader>
          <div className="space-y-4">
            <div><p className="text-sm text-muted-foreground">Review</p><p className="text-sm">{replyReview?.title} - {replyReview?.comment}</p></div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Your Reply</label>
              <Input value={replyText} onChange={e => setReplyText(e.target.value)} placeholder="Type your reply..." />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setReplyReview(null)}>Cancel</Button>
            <Button onClick={() => { if (replyReview) replyMutation.mutate({ id: replyReview.id, adminReply: replyText }); }} disabled={replyMutation.isPending || !replyText.trim()}>{replyMutation.isPending ? "Saving..." : "Reply"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <DialogContent>
          <DialogHeader><DialogTitle>Delete Review</DialogTitle></DialogHeader>
          <p>Are you sure?</p>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteId(null)}>Cancel</Button>
            <Button variant="destructive" onClick={() => { if (deleteId) deleteMutation.mutate(deleteId); }} disabled={deleteMutation.isPending}>Delete</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
