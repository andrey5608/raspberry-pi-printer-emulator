/* Copyright (C) 2018-2021 Artifex Software, Inc.
   All Rights Reserved.

   This software is provided AS-IS with no warranty, either express or
   implied.

   This software is distributed under license and may not be copied,
   modified or distributed except as expressly authorized under the terms
   of the license contained in the file LICENSE in this distribution.

   Refer to licensing information at http://www.artifex.com or contact
   Artifex Software, Inc.,  1305 Grant Avenue - Suite 200, Novato,
   CA 94945, U.S.A., +1(415)492-9861, for further information.
*/

/* Stack operations for the PDF interpreter */

#ifndef PDF_STACK_OPERATIONS
#define PDF_STACK_OPERATIONS

#include "pdf_int.h"
#include "pdf_types.h"
#include "ghostpdf.h"
#include "pdf_obj.h"

int pdfi_pop(pdf_context *ctx, int num);
int pdfi_push(pdf_context *ctx, pdf_obj *o);
int pdfi_mark_stack(pdf_context *ctx, pdf_obj_type type);
void pdfi_clearstack(pdf_context *ctx);
int pdfi_count_to_mark(pdf_context *ctx, uint64_t *count);
int pdfi_clear_to_mark(pdf_context *ctx);

static inline void pdfi_countup_impl(pdf_obj *o)
{
    if (o != NULL) {
        o->refcnt++;
#if REFCNT_DEBUG
    dmprintf3(OBJ_MEMORY(o), "Incrementing reference count of object %d, UID %lu, to %d\n", o->object_num, o->UID, o->refcnt);
#endif
    }
#if REFCNT_DEBUG
    else {
        dprintf("Incrementing reference count of NULL pointer\n");
    }
#endif
}

static inline void pdfi_countdown_impl(pdf_obj *o)
{
    if (o != NULL) {
#ifdef DEBUG
        pdf_context *ctx1 = (pdf_context *)o->ctx;
        if (o->refcnt == 0)
            emprintf(OBJ_MEMORY(o), "Decrementing object with refcount at 0!\n");
#endif
        o->refcnt--;
#if REFCNT_DEBUG
        dmprintf3(OBJ_MEMORY(o), "Decrementing reference count of object %d, UID %lu, to %d\n", o->object_num, o->UID, o->refcnt);
#endif
        if (o->refcnt == 0) {
#if REFCNT_DEBUG
            pdf_context *ctx = (pdf_context *)o->ctx;
            if (ctx != NULL && ctx->cache_entries != 0) {
                pdf_obj_cache_entry *entry = ctx->cache_LRU, *next;

                while(entry) {
                    next = entry->next;
                    if (entry->o->object_num != 0 && entry->o->object_num == o->object_num)
                        dmprintf2(OBJ_MEMORY(o), "Freeing object %d, UID %lu, but there is still a cache entry!\n", o->object_num, o->UID);
                    entry = next;
                }
            }
            dmprintf2(OBJ_MEMORY(o), "Freeing object %d, UID %lu\n", o->object_num, o->UID);
#endif
#ifdef DEBUG
            if (ctx1->xref_table != NULL && o->object_num > 0 &&
                o->object_num < ctx1->xref_table->xref_size &&
                ctx1->xref_table->xref[o->object_num].cache != NULL &&
                ctx1->xref_table->xref[o->object_num].cache->o == o) {
                dmprintf1(OBJ_MEMORY(o), "Freeing object %d while it is still in the object cache!\n", o->object_num);
            }
#endif
            pdfi_free_object(o);
        }
    }
}

/* These two macros are present simply to add a cast to the generic object type, so that
 * we don't get warnings in the implementation routines, the alternative would be to use
 * a cast everywhere we use the inline functions above, or to have them take a void *
 *
 * Ordinarily we would capitalise the name of a macro to differentiate it from a function
 * we make an exception in this case because hte macro descends to an inline function which
 * can be debugged without expanding macros.
 */
#define pdfi_countup(x) pdfi_countup_impl((pdf_obj *)x)

#define pdfi_countdown(x) pdfi_countdown_impl((pdf_obj *)x)

/* Why two functions ? The difference is that when interpreting 'sub' streams
 * such as the content stream for a Form XObject, we may have entries on the
 * stack at the start of the stream interpretation, and we don't want to
 * pop any of those off during the course of the stream. The stack depth stored in
 * the context is used to prevent this in pdfi_pop().
 * This means that, during the course of a stream, the stack top - bottom may
 * not be an accurate reflection of the number of available items on the stack.
 *
 * So pdfi_count_stack() returns the number of available items, and
 * pdfi_count_stack_total() returns the entire size of the stack, and is used to
 * record the saved stack depth when we start a stream.
 *
 * Although these are currently simple calculations, they are abstracted in order
 * to facilitate later replacement if required.
 */
static inline int pdfi_count_total_stack(pdf_context *ctx)
{
    return (ctx->stack_top - ctx->stack_bot);
}

static inline int pdfi_count_stack(pdf_context *ctx)
{
    return (pdfi_count_total_stack(ctx)) - ctx->current_stream_save.stack_count;
}

#endif
