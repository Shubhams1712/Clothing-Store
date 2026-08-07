"use client";

import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Upload, Image as ImageIcon } from "lucide-react";

export default function AdminMediaPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Media Library</h1>
        <Button><Upload className="mr-2 h-4 w-4" />Upload</Button>
      </div>
      <Card>
        <CardContent className="p-6">
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <ImageIcon className="h-12 w-12 text-muted-foreground mb-4" />
            <h3 className="text-lg font-medium">Media Library</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Upload and manage product images, banners, and collection images.
            </p>
            <p className="text-xs text-muted-foreground mt-4">
              Cloudinary integration will be configured in settings.
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
